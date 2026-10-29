using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OnlineShoppingWebsite.Models
{
    // Một dòng sản phẩm trong giỏ hàng.
    // Không lưu trực tiếp object Product của EF vào Session.
    public class CartItem
    {
        public int ProductID { get; set; }
        public string ProductName { get; set; }
        public string ImageUrl { get; set; }
        // Giá được ghi nhận khi sản phẩm được thêm vào giỏ.
        public decimal UnitPrice { get; set; }
        // Số lượng khách muốn mua.
        public int Quantity { get; set; }
        // Tồn kho tại thời điểm thêm/cập nhật giỏ.
        public int StockQuantity { get; set; }
        // Thành tiền của một dòng.
        public decimal LineTotal
        {
            get { return UnitPrice * Quantity; }
        }
    }

    // Toàn bộ giỏ hàng.
    public class Cart
    {
        // Danh sách nội bộ của giỏ.
        private readonly List<CartItem> items = new List<CartItem>();

        // View chỉ được đọc danh sách, không thay trực tiếp.
        public IEnumerable<CartItem> Items
        {
            get { return items; }
        }

        // Thêm một hoặc nhiều Product vào giỏ.
        // Trả true nếu thêm/tăng số lượng thành công.
        public bool Add(Product product, int quantity = 1)
        {
            if (product == null
                || !product.IsActive
                || product.StockQuantity <= 0
                || quantity <= 0)
            {
                return false;
            }

            // Tìm xem Product đã có trong giỏ chưa.
            var item = items.FirstOrDefault(x => x.ProductID == product.ProductID);
            if (item == null)
            {
                if (quantity > product.StockQuantity)
                {
                    return false;
                }

                // Chưa có -> tạo dòng mới với Quantity được chỉ định.
                items.Add(new CartItem
                {
                    ProductID = product.ProductID,
                    ProductName = product.ProductName,
                    ImageUrl = product.ImageUrl,
                    UnitPrice = product.Price,
                    Quantity = quantity,
                    StockQuantity = product.StockQuantity
                });
                return true;
            }

            // Đã có -> chỉ tăng nếu tổng số lượng không vượt tồn kho.
            if (item.Quantity + quantity <= product.StockQuantity)
            {
                item.Quantity += quantity;
                item.StockQuantity = product.StockQuantity;
                return true;
            }

            return false;
        }

        // Cập nhật số lượng một dòng.
        public bool Update(int productId, int quantity, int currentStock)
        {
            var item = items.FirstOrDefault(x => x.ProductID == productId);
            if (item == null)
            {
                return false;
            }

            // Không nhận số lượng <= 0 hoặc vượt tồn kho.
            if (quantity <= 0 || quantity > currentStock)
            {
                return false;
            }

            item.Quantity = quantity;
            item.StockQuantity = currentStock;
            return true;
        }

        // Xóa một Product khỏi giỏ.
        public void Remove(int productId)
        {
            items.RemoveAll(x => x.ProductID == productId);
        }

        // Tổng số lượng sản phẩm trong giỏ.
        public int TotalQuantity()
        {
            return items.Sum(x => x.Quantity);
        }

        // Tổng tiền của toàn bộ giỏ.
        public decimal TotalMoney()
        {
            return items.Sum(x => x.LineTotal);
        }

        // Xóa toàn bộ giỏ sau khi đặt hàng thành công hoặc khi người dùng làm trống giỏ.
        public void Clear()
        {
            items.Clear();
        }
    }
}
