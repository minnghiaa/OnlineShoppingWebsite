using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using OnlineShoppingWebsite.Filters;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    // =========================================================
    // ADMIN DASHBOARD CONTROLLER (BUỔI 08 - MỤC 6.5)
    // =========================================================
    // Gắn trên class: mọi action trong AdminController
    // đều phải qua kiểm tra quyền AdminAuthorize.
    [AdminAuthorize]
    public class AdminController : Controller
    {
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // GET: /Admin
        public ActionResult Index()
        {
            // Thống kê tổng quan phục vụ trang Dashboard Quản trị
            ViewBag.TotalProducts = db.Products.Count();
            ViewBag.ActiveProducts = db.Products.Count(p => p.IsActive);
            ViewBag.TotalCategories = db.Categories.Count();
            ViewBag.TotalOrders = db.OrderProes.Count();
            ViewBag.PendingOrders = db.OrderProes.Count(o => o.Status == "Chờ xử lý" || o.Status == "Pending");
            ViewBag.CompletedOrders = db.OrderProes.Count(o => o.Status == "Hoàn thành" || o.Status == "Completed");

            // Tổng doanh thu từ các đơn hàng thành công
            var completedOrders = db.OrderProes
                .Where(o => o.Status == "Hoàn thành" || o.Status == "Completed")
                .Select(o => (decimal?)o.TotalAmount)
                .Sum();
            ViewBag.TotalRevenue = completedOrders ?? 0;

            // Lấy 5 đơn hàng mới nhất (eager load Customer để hiển thị đầy đủ thông tin người nhận)
            ViewBag.RecentOrders = db.OrderProes
                .Include(o => o.Customer)
                .OrderByDescending(o => o.OrderDate)
                .Take(5)
                .ToList();

            return View();
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
