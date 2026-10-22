using System;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Net;
using System.Web;
using System.Web.Mvc;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    public class ProductsController : Controller
    {
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // GET: Products (Trang quản lý danh sách sản phẩm)
        public ActionResult Index()
        {
            var products = db.Products.Include(p => p.Category).OrderByDescending(p => p.ProductID).ToList();
            return View(products);
        }

        // GET: Products/Details/5 (Xem chi tiết sản phẩm)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // GET: Products/Create (Mở form thêm sản phẩm mới)
        public ActionResult Create()
        {
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName");
            return View();
        }

        // POST: Products/Create (Lưu sản phẩm mới kèm upload file ảnh)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create(
            [Bind(Include = "ProductName,Description,CategoryID,Price,StockQuantity,IsActive")] Product product,
            HttpPostedFileBase imageFile)
        {
            // Kiểm tra và xử lý file ảnh upload
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(imageFile.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageUrl", "Chỉ chấp nhận file ảnh định dạng JPG, JPEG hoặc PNG.");
                }
                else if (imageFile.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageUrl", "Dung lượng ảnh tối đa là 2 MB.");
                }
                else
                {
                    // Lưu ảnh vật lý và gán tên file vào Product.ImageUrl
                    product.ImageUrl = SaveImage(imageFile);
                }
            }
            else
            {
                // Nếu không upload ảnh, dùng ảnh mặc định
                product.ImageUrl = "img-01.jpg";
            }

            if (ModelState.IsValid)
            {
                db.Products.Add(product);
                db.SaveChanges();
                TempData["Success"] = "Thêm sản phẩm mới \"" + product.ProductName + "\" thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Edit/5 (Mở form sửa sản phẩm)
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Find(id);
            if (product == null)
            {
                return HttpNotFound();
            }
            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // POST: Products/Edit/5 (Lưu thay đổi sản phẩm, giữ ảnh cũ hoặc cập nhật ảnh mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit(
            [Bind(Include = "ProductID,ProductName,Description,CategoryID,Price,StockQuantity,IsActive")] Product product,
            HttpPostedFileBase imageFile)
        {
            // Lấy sản phẩm cũ từ database để biết tên ảnh hiện tại
            var oldProduct = db.Products.AsNoTracking().FirstOrDefault(p => p.ProductID == product.ProductID);
            if (oldProduct == null)
            {
                return HttpNotFound();
            }

            // Mặc định giữ lại ảnh cũ
            product.ImageUrl = oldProduct.ImageUrl;

            // Nếu người dùng chọn ảnh mới
            if (imageFile != null && imageFile.ContentLength > 0)
            {
                string extension = Path.GetExtension(imageFile.FileName).ToLower();
                string[] allowedExtensions = { ".jpg", ".jpeg", ".png" };

                if (!allowedExtensions.Contains(extension))
                {
                    ModelState.AddModelError("ImageUrl", "Chỉ chấp nhận file ảnh định dạng JPG, JPEG hoặc PNG.");
                }
                else if (imageFile.ContentLength > 2 * 1024 * 1024)
                {
                    ModelState.AddModelError("ImageUrl", "Dung lượng ảnh tối đa là 2 MB.");
                }
                else
                {
                    product.ImageUrl = SaveImage(imageFile);
                }
            }

            if (ModelState.IsValid)
            {
                db.Entry(product).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật sản phẩm \"" + product.ProductName + "\" thành công!";
                return RedirectToAction("Index");
            }

            ViewBag.CategoryID = new SelectList(db.Categories, "CategoryID", "CategoryName", product.CategoryID);
            return View(product);
        }

        // GET: Products/Delete/5 (Mở trang xác nhận xóa sản phẩm)
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Product product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id);
            if (product == null)
            {
                return HttpNotFound();
            }
            return View(product);
        }

        // POST: Products/Delete/5 (Xóa sản phẩm khỏi CSDL)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Product product = db.Products.Find(id);
            if (product != null)
            {
                string name = product.ProductName;
                db.Products.Remove(product);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa sản phẩm \"" + name + "\" thành công!";
            }
            return RedirectToAction("Index");
        }

        // Hàm dùng chung: lưu file ảnh vào Content/images và tránh trùng tên file
        private string SaveImage(HttpPostedFileBase imageFile)
        {
            string originalFileName = Path.GetFileName(imageFile.FileName);
            string fileNameWithoutExtension = Path.GetFileNameWithoutExtension(originalFileName);
            string extension = Path.GetExtension(originalFileName).ToLower();

            string folder = Server.MapPath("~/Content/images");
            if (!Directory.Exists(folder))
            {
                Directory.CreateDirectory(folder);
            }

            string fileName = fileNameWithoutExtension + extension;
            string path = Path.Combine(folder, fileName);

            // Tự động thêm hậu tố -1, -2 nếu file cùng tên đã tồn tại
            int count = 1;
            while (System.IO.File.Exists(path))
            {
                fileName = fileNameWithoutExtension + "-" + count + extension;
                path = Path.Combine(folder, fileName);
                count++;
            }

            imageFile.SaveAs(path);
            return fileName;
        }

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                db.Dispose();
            }
            base.Dispose(disposing);
        }
    }
}
