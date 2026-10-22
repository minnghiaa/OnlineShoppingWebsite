using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net;
using System.Web.Mvc;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    public class StoreController : Controller
    {
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // GET: Store (Trang chủ cửa hàng, danh mục động, tìm kiếm LINQ, lọc giá, sắp xếp, phân trang)
        public ActionResult Index(int? categoryId, string search, decimal? minPrice, decimal? maxPrice, string sort, int page = 1)
        {
            // Chỉ lấy các sản phẩm đang hoạt động
            var query = db.Products.Include(p => p.Category).Where(p => p.IsActive);

            // 1. Lọc theo danh mục
            if (categoryId.HasValue && categoryId.Value > 0)
            {
                query = query.Where(p => p.CategoryID == categoryId.Value);
            }

            // 2. Tìm kiếm LINQ theo tên hoặc mô tả sản phẩm
            if (!string.IsNullOrWhiteSpace(search))
            {
                search = search.Trim();
                query = query.Where(p => p.ProductName.Contains(search) || (p.Description != null && p.Description.Contains(search)));
            }

            // 3. Lọc theo khoảng giá
            if (minPrice.HasValue && minPrice.Value >= 0)
            {
                query = query.Where(p => p.Price >= minPrice.Value);
            }
            if (maxPrice.HasValue && maxPrice.Value >= 0)
            {
                query = query.Where(p => p.Price <= maxPrice.Value);
            }

            // 4. Sắp xếp giá và thời gian
            switch (sort)
            {
                case "price_asc":
                    query = query.OrderBy(p => p.Price);
                    break;
                case "price_desc":
                    query = query.OrderByDescending(p => p.Price);
                    break;
                case "name_asc":
                    query = query.OrderBy(p => p.ProductName);
                    break;
                default:
                    // Mặc định: Sản phẩm mới nhất lên trước
                    query = query.OrderByDescending(p => p.ProductID);
                    break;
            }

            // 5. Phân trang (8 sản phẩm / trang)
            int pageSize = 8;
            int totalItems = query.Count();
            int totalPages = (int)Math.Ceiling((double)totalItems / pageSize);
            if (totalPages < 1) totalPages = 1;
            if (page < 1) page = 1;
            if (page > totalPages) page = totalPages;

            var products = query.Skip((page - 1) * pageSize).Take(pageSize).ToList();

            var viewModel = new StoreIndexViewModel
            {
                Products = products,
                Categories = db.Categories.OrderBy(c => c.CategoryID).ToList(),
                CurrentCategoryID = categoryId,
                SearchTerm = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                CurrentSort = sort,
                CurrentPage = page,
                TotalPages = totalPages,
                TotalItems = totalItems,
                PageSize = pageSize
            };

            return View(viewModel);
        }

        // GET: Store/Details/5 (Xem chi tiết sản phẩm và kiểm tra số lượng tồn kho)
        public ActionResult Details(int? id)
        {
            if (id == null)
            {
                return new HttpStatusCodeResult(HttpStatusCode.BadRequest);
            }

            var product = db.Products.Include(p => p.Category).FirstOrDefault(p => p.ProductID == id && p.IsActive);
            if (product == null)
            {
                return HttpNotFound();
            }

            // Lấy 4 sản phẩm liên quan cùng danh mục (ngoại trừ sản phẩm hiện tại)
            ViewBag.RelatedProducts = db.Products
                .Include(p => p.Category)
                .Where(p => p.CategoryID == product.CategoryID && p.ProductID != product.ProductID && p.IsActive)
                .Take(4)
                .ToList();

            return View(product);
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
