using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Web;
using System.Web.Mvc;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    public class ShoppingCartController : Controller
    {
        // Kết nối CSDL để đọc và lưu dữ liệu.
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // Lấy Cart hiện tại từ Session.
        // Nếu chưa có thì tạo Cart mới.
        private Cart GetCart()
        {
            var cart = Session["Cart"] as Cart;
            if (cart == null)
            {
                cart = new Cart();
                Session["Cart"] = cart;
            }
            return cart;
        }

        // =========================================================
        // CHỨC NĂNG 1, 2, 3: HIỂN THỊ GIỎ HÀNG
        // =========================================================
        public ActionResult ShowCart()
        {
            return View(GetCart());
        }

        // =========================================================
        // CHỨC NĂNG 2: THÊM VÀO GIỎ HÀNG
        // Hỗ trợ cả GET từ link "Mua" (tham số id)
        // và POST từ Form trang Chi tiết sản phẩm (tham số productId & quantity)
        // =========================================================
        [AcceptVerbs(HttpVerbs.Get | HttpVerbs.Post)]
        public ActionResult AddToCart(int? id, int? productId, int quantity = 1)
        {
            int actualId = id ?? productId ?? 0;
            if (actualId <= 0)
            {
                TempData["CartError"] = "Sản phẩm không hợp lệ.";
                return RedirectToAction("Index", "Store");
            }

            if (quantity <= 0) quantity = 1;

            // Chỉ cho thêm Product đang tồn tại và đang bán.
            var product = db.Products.FirstOrDefault(p => p.ProductID == actualId && p.IsActive);
            if (product == null)
            {
                TempData["CartError"] = "Sản phẩm không tồn tại hoặc đã ngừng bán.";
                return RedirectToAction("Index", "Store");
            }

            if (product.StockQuantity <= 0)
            {
                TempData["CartError"] = "Sản phẩm \"" + product.ProductName + "\" hiện đã hết hàng.";
                return RedirectToAction("ShowCart");
            }

            // Cart.Add tự kiểm tra tồn kho.
            bool added = GetCart().Add(product, quantity);
            if (!added)
            {
                TempData["CartError"] = "Không thể thêm sản phẩm. Số lượng yêu cầu (" + quantity + ") vượt quá tồn kho còn lại (" + product.StockQuantity + " sản phẩm).";
            }
            else
            {
                string msg = quantity > 1
                    ? "Đã thêm " + quantity + " sản phẩm \"" + product.ProductName + "\" vào giỏ hàng thành công!"
                    : "Đã thêm \"" + product.ProductName + "\" vào giỏ hàng thành công!";
                TempData["CartSuccess"] = msg;
            }

            // Sau khi thêm, mở giỏ để người dùng nhìn thấy kết quả.
            return RedirectToAction("ShowCart");
        }

        // =========================================================
        // CHỨC NĂNG 3: CẬP NHẬT SỐ LƯỢNG SẢN PHẨM TRONG GIỎ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateQuantity(int productId, int quantity)
        {
            // Đọc lại Product từ CSDL theo ProductID để lấy StockQuantity mới nhất trong database.
            var product = db.Products.Find(productId);
            if (product == null)
            {
                TempData["CartError"] = "Không tìm thấy sản phẩm.";
                return RedirectToAction("ShowCart");
            }

            // Lấy Cart hiện tại từ Session rồi gọi hàm Update().
            bool updated = GetCart().Update(productId, quantity, product.StockQuantity);
            if (!updated)
            {
                TempData["CartError"] = "Số lượng phải lớn hơn 0 và không vượt tồn kho (" + product.StockQuantity + " sản phẩm).";
            }
            else
            {
                TempData["CartSuccess"] = "Cập nhật số lượng thành công!";
            }

            return RedirectToAction("ShowCart");
        }

        // =========================================================
        // CHỨC NĂNG 3: XÓA SẢN PHẨM KHỎI GIỎ
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Remove(int productId)
        {
            GetCart().Remove(productId);
            TempData["CartSuccess"] = "Đã xóa sản phẩm khỏi giỏ hàng.";
            return RedirectToAction("ShowCart");
        }

        // =========================================================
        // BÀI TẬP CUỐI BUỔI 07 (BÀI 1): XÓA TOÀN BỘ GIỎ HÀNG
        // =========================================================
        public ActionResult ClearCart()
        {
            GetCart().Clear();
            TempData["CartSuccess"] = "Đã làm trống giỏ hàng.";
            return RedirectToAction("ShowCart");
        }

        // =========================================================
        // CHỨC NĂNG 4: HIỂN THỊ THÔNG TIN GIỎ TRÊN NAVBAR
        // Action này chỉ được gọi từ View/Layout, không cho truy cập trực tiếp bằng URL.
        // =========================================================
        [ChildActionOnly]
        public PartialViewResult BagCart()
        {
            var cart = Session["Cart"] as Cart;
            ViewBag.CartQty = cart == null ? 0 : cart.TotalQuantity();
            ViewBag.CartTotal = cart == null ? 0m : cart.TotalMoney();
            return PartialView();
        }

        // =========================================================
        // CHỨC NĂNG 5 & 6: GUEST CHECKOUT (MỞ FORM NHẬP THÔNG TIN)
        // =========================================================
        public ActionResult Checkout()
        {
            var cart = GetCart();
            // Nếu giỏ trống thì quay lại trang ShowCart.
            if (!cart.Items.Any())
            {
                TempData["CartError"] = "Giỏ hàng đang trống. Vui lòng chọn sản phẩm trước khi thanh toán.";
                return RedirectToAction("ShowCart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart
            };

            return View(model);
        }

        // =========================================================
        // CHỨC NĂNG 6: GUEST CHECKOUT (NHẬN DỮ LIỆU VÀ LƯU ĐƠN HÀNG)
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Checkout(CheckoutViewModel model)
        {
            var cart = GetCart();
            model.Cart = cart;

            // Không cho đặt hàng nếu giỏ đã trống.
            if (!cart.Items.Any())
            {
                TempData["CartError"] = "Giỏ hàng của bạn đang trống!";
                return RedirectToAction("ShowCart");
            }

            // Kiểm tra dữ liệu hợp lệ theo DataAnnotations
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Kiểm tra lại tồn kho ngay trước khi tạo đơn
            foreach (var item in cart.Items)
            {
                var product = db.Products.Find(item.ProductID);
                if (product == null
                    || !product.IsActive
                    || product.StockQuantity < item.Quantity)
                {
                    int available = product != null ? product.StockQuantity : 0;
                    ModelState.AddModelError("", "Sản phẩm \"" + item.ProductName + "\" không còn đủ tồn kho (chỉ còn " + available + " sản phẩm). Vui lòng cập nhật lại giỏ hàng.");
                    return View(model);
                }
            }

            // Bắt đầu Transaction bảo đảm: Customer + OrderPro + OrderDetail + cập nhật tồn kho cùng thành công
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 1. TẠO CUSTOMER MỚI CHO KHÁCH VÃNG LAI (UserID = NULL)
                    var customer = new Customer
                    {
                        UserID = null,
                        FullName = model.FullName.Trim(),
                        Phone = model.Phone.Trim(),
                        Email = model.Email.Trim(),
                        Address = model.Address.Trim()
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();

                    // 2. TẠO ORDERPRO
                    var order = new OrderPro
                    {
                        OrderDate = DateTime.Now,
                        CustomerID = customer.CustomerID,
                        RecipientName = model.FullName.Trim(),
                        ShippingPhone = model.Phone.Trim(),
                        ShippingAddress = model.ShippingAddress.Trim(),
                        TotalAmount = cart.TotalMoney(),
                        Status = "Chờ xử lý"
                    };
                    db.OrderProes.Add(order);
                    db.SaveChanges();

                    // 3. TẠO CÁC ORDERDETAIL VÀ TRỪ TỒN KHO
                    foreach (var item in cart.Items)
                    {
                        var product = db.Products.Find(item.ProductID);
                        var detail = new OrderDetail
                        {
                            OrderID = order.OrderID,
                            ProductID = item.ProductID,
                            Quantity = item.Quantity,
                            UnitPrice = item.UnitPrice
                        };
                        db.OrderDetails.Add(detail);

                        // Trừ số lượng tồn kho
                        product.StockQuantity -= item.Quantity;
                    }
                    db.SaveChanges();

                    // Xác nhận lưu toàn bộ thay đổi
                    transaction.Commit();

                    // Xóa giỏ hàng sau khi lưu thành công
                    cart.Clear();

                    // Chuyển sang trang xem kết quả đơn hàng
                    return RedirectToAction("OrderSuccess", new { id = order.OrderID });
                }
                catch (System.Data.Entity.Validation.DbEntityValidationException ex)
                {
                    transaction.Rollback();
                    var errorMessages = ex.EntityValidationErrors
                        .SelectMany(x => x.ValidationErrors)
                        .Select(x => x.ErrorMessage);
                    var fullErrorMessage = string.Join("; ", errorMessages);
                    ModelState.AddModelError("", "Lỗi kiểm tra dữ liệu: " + fullErrorMessage);
                    return View(model);
                }
                catch (Exception ex)
                {
                    // Hủy toàn bộ thay đổi nếu có lỗi
                    transaction.Rollback();
                    string detail = ex.GetBaseException().Message;
                    ModelState.AddModelError("", "Có lỗi khi lưu đơn hàng: " + detail + ". Vui lòng thử lại!");
                    return View(model);
                }
            }
        }

        // =========================================================
        // CHỨC NĂNG 7: HIỂN THỊ TRANG ĐẶT HÀNG THÀNH CÔNG
        // =========================================================
        public ActionResult OrderSuccess(int id)
        {
            // Lấy OrderPro theo OrderID cùng Customer, OrderDetail và Product
            var order = db.OrderProes
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            return View(order);
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
