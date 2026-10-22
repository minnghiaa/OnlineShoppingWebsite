using System;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    public class CategoriesController : Controller
    {
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // GET: Categories (Danh sách quản lý danh mục)
        public ActionResult Index()
        {
            var categories = db.Categories.Include(c => c.Products).OrderBy(c => c.CategoryID).ToList();
            return View(categories);
        }

        // GET: Categories/Details/5 (Chi tiết danh mục và danh sách sản phẩm trực thuộc)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // GET: Categories/Create (Mở form thêm danh mục mới)
        public ActionResult Create()
        {
            return View();
        }

        // POST: Categories/Create (Lưu danh mục mới)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Create([Bind(Include = "CategoryName")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục không được để trống.");
            }

            if (ModelState.IsValid)
            {
                db.Categories.Add(category);
                db.SaveChanges();
                TempData["Success"] = "Thêm danh mục \"" + category.CategoryName + "\" thành công!";
                return RedirectToAction("Index");
            }

            return View(category);
        }

        // GET: Categories/Edit/5 (Mở form chỉnh sửa tên danh mục)
        public ActionResult Edit(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Find(id);
            if (category == null)
            {
                return HttpNotFound();
            }
            return View(category);
        }

        // POST: Categories/Edit/5 (Lưu cập nhật tên danh mục)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Edit([Bind(Include = "CategoryID,CategoryName")] Category category)
        {
            if (string.IsNullOrWhiteSpace(category.CategoryName))
            {
                ModelState.AddModelError("CategoryName", "Tên danh mục không được để trống.");
            }

            if (ModelState.IsValid)
            {
                db.Entry(category).State = EntityState.Modified;
                db.SaveChanges();
                TempData["Success"] = "Cập nhật danh mục \"" + category.CategoryName + "\" thành công!";
                return RedirectToAction("Index");
            }
            return View(category);
        }

        // GET: Categories/Delete/5 (Mở trang xác nhận xóa danh mục)
        public ActionResult Delete(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }
            Category category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryID == id);
            if (category == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra ràng buộc: Nếu danh mục đã có sản phẩm thuộc về, thông báo cảnh báo
            if (category.Products != null && category.Products.Any())
            {
                ViewBag.HasProducts = true;
                ViewBag.ProductCount = category.Products.Count();
            }
            else
            {
                ViewBag.HasProducts = false;
            }

            return View(category);
        }

        // POST: Categories/Delete/5 (Xóa danh mục khỏi CSDL)
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public ActionResult DeleteConfirmed(int id)
        {
            Category category = db.Categories.Include(c => c.Products).FirstOrDefault(c => c.CategoryID == id);
            if (category != null)
            {
                if (category.Products != null && category.Products.Any())
                {
                    TempData["Error"] = "Không thể xóa danh mục \"" + category.CategoryName + "\" vì đang có " + category.Products.Count() + " sản phẩm thuộc danh mục này!";
                    return RedirectToAction("Index");
                }

                string name = category.CategoryName;
                db.Categories.Remove(category);
                db.SaveChanges();
                TempData["Success"] = "Đã xóa danh mục \"" + name + "\" thành công!";
            }
            return RedirectToAction("Index");
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
