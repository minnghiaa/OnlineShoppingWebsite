using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using OnlineShoppingWebsite.Filters;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    // =========================================================
    // ORDERS CONTROLLER (BUỔI 08 - BÀI TẬP CUỐI BUỔI 2 & 3)
    // Quản lý đơn hàng, cập nhật trạng thái và xem chi tiết đơn hàng
    // =========================================================
    [AdminAuthorize]
    public class OrdersController : Controller
    {
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // Danh sách các trạng thái đơn hàng chuẩn
        private readonly string[] validStatuses = new string[]
        {
            "Chờ xử lý",
            "Đã xác nhận",
            "Đang giao",
            "Hoàn thành",
            "Đã hủy"
        };

        // =========================================================
        // BÀI 2: HIỂN THỊ DANH SÁCH ĐƠN HÀNG + TÌM KIẾM & BỘ LỌC
        // GET: /Orders
        // =========================================================
        public ActionResult Index(string status, string search)
        {
            var query = db.OrderProes
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails)
                .AsQueryable();

            // 1. Tìm kiếm theo mã đơn, người nhận hoặc số điện thoại
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                int searchId;
                bool isId = int.TryParse(search.Replace("#", ""), out searchId);

                if (isId)
                {
                    query = query.Where(o => o.OrderID == searchId
                        || (o.RecipientName != null && o.RecipientName.Contains(search))
                        || (o.ShippingPhone != null && o.ShippingPhone.Contains(search)));
                }
                else
                {
                    query = query.Where(o => (o.RecipientName != null && o.RecipientName.Contains(search))
                        || (o.ShippingPhone != null && o.ShippingPhone.Contains(search)));
                }
            }

            // 2. Lọc theo trạng thái
            if (!string.IsNullOrWhiteSpace(status) && status != "All")
            {
                if (status == "Chờ xử lý" || status == "Pending")
                {
                    query = query.Where(o => o.Status == "Chờ xử lý" || o.Status == "Pending");
                }
                else if (status == "Đã xác nhận" || status == "Confirmed")
                {
                    query = query.Where(o => o.Status == "Đã xác nhận" || o.Status == "Confirmed");
                }
                else if (status == "Đang giao" || status == "Shipping")
                {
                    query = query.Where(o => o.Status == "Đang giao" || o.Status == "Shipping");
                }
                else if (status == "Hoàn thành" || status == "Completed")
                {
                    query = query.Where(o => o.Status == "Hoàn thành" || o.Status == "Completed");
                }
                else if (status == "Đã hủy" || status == "Cancelled")
                {
                    query = query.Where(o => o.Status == "Đã hủy" || o.Status == "Cancelled");
                }
                else
                {
                    query = query.Where(o => o.Status == status);
                }
            }

            // Sắp xếp đơn mới nhất trước
            var orders = query.OrderByDescending(o => o.OrderDate).ToList();

            // Thống kê số lượng đơn theo trạng thái phục vụ thanh tab bộ lọc
            ViewBag.CountAll = db.OrderProes.Count();
            ViewBag.CountPending = db.OrderProes.Count(o => o.Status == "Chờ xử lý" || o.Status == "Pending");
            ViewBag.CountConfirmed = db.OrderProes.Count(o => o.Status == "Đã xác nhận" || o.Status == "Confirmed");
            ViewBag.CountShipping = db.OrderProes.Count(o => o.Status == "Đang giao" || o.Status == "Shipping");
            ViewBag.CountCompleted = db.OrderProes.Count(o => o.Status == "Hoàn thành" || o.Status == "Completed");
            ViewBag.CountCancelled = db.OrderProes.Count(o => o.Status == "Đã hủy" || o.Status == "Cancelled");

            ViewBag.CurrentStatus = status ?? "All";
            ViewBag.CurrentSearch = search;
            ViewBag.StatusList = validStatuses;

            return View(orders);
        }

        // =========================================================
        // BÀI 2: CẬP NHẬT TRẠNG THÁI ĐƠN HÀNG (STATUS)
        // POST: /Orders/UpdateStatus
        // =========================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult UpdateStatus(int id, string status, string returnUrl)
        {
            var order = db.OrderProes.Find(id);
            if (order == null)
            {
                return HttpNotFound();
            }

            // Kiểm tra trạng thái hợp lệ
            if (validStatuses.Contains(status) || status == "Pending" || status == "Confirmed" || status == "Shipping" || status == "Completed" || status == "Cancelled")
            {
                string oldStatus = order.Status;
                order.Status = status;
                db.SaveChanges();

                TempData["Success"] = "Đã cập nhật trạng thái đơn hàng #" + id + " sang \"" + status + "\" thành công!";
            }
            else
            {
                TempData["Error"] = "Trạng thái \"" + status + "\" không hợp lệ!";
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index");
        }

        // =========================================================
        // BÀI 3: XEM CHI TIẾT ĐƠN HÀNG
        // GET: /Orders/Details/5
        // =========================================================
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            // Lấy OrderPro cùng Customer, OrderDetail và Product
            var order = db.OrderProes
                .Include(o => o.Customer)
                .Include(o => o.OrderDetails.Select(d => d.Product))
                .FirstOrDefault(o => o.OrderID == id);

            if (order == null)
            {
                return HttpNotFound();
            }

            ViewBag.StatusList = validStatuses;
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
