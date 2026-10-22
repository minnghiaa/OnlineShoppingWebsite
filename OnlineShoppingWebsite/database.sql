-- ============================================================================
-- ĐỒ ÁN LẬP TRÌNH WEB - ONLINE SHOPPING WEBSITE (CHỦ ĐỀ BÁN QUẦN ÁO THỜI TRANG)
-- HỆ QUẢN TRỊ CƠ SỞ DỮ LIỆU: SQL SERVER
-- TÊN DATABASE: OnlineShoppingDB
-- ============================================================================

USE master;
GO

-- 1. KIỂM TRA VÀ TẠO MỚI DATABASE
IF EXISTS (SELECT name FROM sys.databases WHERE name = N'OnlineShoppingDB')
BEGIN
    ALTER DATABASE OnlineShoppingDB SET SINGLE_USER WITH ROLLBACK IMMEDIATE;
    DROP DATABASE OnlineShoppingDB;
    PRINT N'Đã xóa cơ sở dữ liệu cũ OnlineShoppingDB thành công.';
END
GO

CREATE DATABASE OnlineShoppingDB;
GO

USE OnlineShoppingDB;
GO

PRINT N'Đang khởi tạo các bảng và ràng buộc toàn vẹn...';
GO

-- ============================================================================
-- 2. TẠO CẤU TRÚC 7 BẢNG DỮ LIỆU (CÓ ĐẦY ĐỦ PK, FK, CHECK CONSTRAINTS)
-- ============================================================================

-- Bảng 1: Role (Phân quyền: Admin, Customer)
CREATE TABLE [Role] (
    RoleID INT IDENTITY(1,1) PRIMARY KEY,
    RoleName NVARCHAR(50) NOT NULL UNIQUE
);
GO

-- Bảng 2: UserAccount (Tài khoản người dùng với PasswordHash & Salt)
CREATE TABLE UserAccount (
    UserID INT IDENTITY(1,1) PRIMARY KEY,
    UserName NVARCHAR(50) NOT NULL UNIQUE,
    PasswordHash NVARCHAR(256) NOT NULL,
    PasswordSalt NVARCHAR(256) NOT NULL,
    RoleID INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CreatedAt DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT FK_UserAccount_Role FOREIGN KEY (RoleID) REFERENCES [Role](RoleID)
);
GO

-- Bảng 3: Customer (Thông tin chi tiết của khách hàng)
CREATE TABLE Customer (
    CustomerID INT IDENTITY(1,1) PRIMARY KEY,
    UserID INT NOT NULL UNIQUE,
    FullName NVARCHAR(100) NOT NULL,
    Phone NVARCHAR(20) NULL,
    Email NVARCHAR(100) NULL,
    Address NVARCHAR(255) NULL,
    CONSTRAINT FK_Customer_UserAccount FOREIGN KEY (UserID) REFERENCES UserAccount(UserID) ON DELETE CASCADE
);
GO

-- Bảng 4: Category (Danh mục sản phẩm)
CREATE TABLE Category (
    CategoryID INT IDENTITY(1,1) PRIMARY KEY,
    CategoryName NVARCHAR(100) NOT NULL
);
GO

-- Bảng 5: Product (Sản phẩm thời trang)
-- Kiểm tra Price >= 0 và StockQuantity >= 0
CREATE TABLE Product (
    ProductID INT IDENTITY(1,1) PRIMARY KEY,
    ProductName NVARCHAR(200) NOT NULL,
    [Description] NVARCHAR(MAX) NULL,
    CategoryID INT NOT NULL,
    Price DECIMAL(18,2) NOT NULL,
    ImageUrl NVARCHAR(255) NULL,
    StockQuantity INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    CONSTRAINT CK_Product_Price CHECK (Price >= 0),
    CONSTRAINT CK_Product_StockQuantity CHECK (StockQuantity >= 0),
    CONSTRAINT FK_Product_Category FOREIGN KEY (CategoryID) REFERENCES Category(CategoryID)
);
GO

-- Bảng 6: OrderPro (Đơn đặt hàng)
-- Status chỉ nhận 1 trong 5 trạng thái hợp lệ theo luồng nghiệp vụ
CREATE TABLE OrderPro (
    OrderID INT IDENTITY(1,1) PRIMARY KEY,
    OrderDate DATETIME NOT NULL DEFAULT GETDATE(),
    CustomerID INT NOT NULL,
    ShippingAddress NVARCHAR(255) NOT NULL,
    TotalAmount DECIMAL(18,2) NOT NULL DEFAULT 0,
    [Status] NVARCHAR(50) NOT NULL DEFAULT N'Pending',
    CONSTRAINT CK_OrderPro_TotalAmount CHECK (TotalAmount >= 0),
    CONSTRAINT CK_OrderPro_Status CHECK ([Status] IN (N'Pending', N'Confirmed', N'Shipping', N'Completed', N'Cancelled')),
    CONSTRAINT FK_OrderPro_Customer FOREIGN KEY (CustomerID) REFERENCES Customer(CustomerID)
);
GO

-- Bảng 7: OrderDetail (Chi tiết đơn hàng)
-- Kiểm tra Quantity > 0 và UnitPrice >= 0
CREATE TABLE OrderDetail (
    OrderDetailID INT IDENTITY(1,1) PRIMARY KEY,
    OrderID INT NOT NULL,
    ProductID INT NOT NULL,
    Quantity INT NOT NULL,
    UnitPrice DECIMAL(18,2) NOT NULL,
    CONSTRAINT CK_OrderDetail_Quantity CHECK (Quantity > 0),
    CONSTRAINT CK_OrderDetail_UnitPrice CHECK (UnitPrice >= 0),
    CONSTRAINT FK_OrderDetail_OrderPro FOREIGN KEY (OrderID) REFERENCES OrderPro(OrderID) ON DELETE CASCADE,
    CONSTRAINT FK_OrderDetail_Product FOREIGN KEY (ProductID) REFERENCES Product(ProductID)
);
GO

-- ============================================================================
-- 3. CHÈN DỮ LIỆU MẪU (SEED DATA)
-- ============================================================================

-- 3.1. Dữ liệu bảng Role
INSERT INTO [Role] (RoleName) VALUES (N'Admin');
INSERT INTO [Role] (RoleName) VALUES (N'Customer');
GO

-- 3.2. Dữ liệu bảng UserAccount (Mật khẩu được mã hóa SHA-256 kèm Salt)
-- Mật khẩu mặc định:
--   admin      : Admin@123
--   customer1  : Customer@123
--   customer2  : Customer@123
INSERT INTO UserAccount (UserName, PasswordHash, PasswordSalt, RoleID, IsActive, CreatedAt)
VALUES 
(N'admin', N'GkVmNsIA3PmpFvhyt1CaayNqYiTbs9XMT9hQVOYJAVY=', N'salt_admin_2026', 1, 1, GETDATE()),
(N'customer1', N'Z/SQL9raudgpJ5hSP9+OQfVHbpKEAdxMfaKzWpXl31M=', N'salt_cust1_2026', 2, 1, GETDATE()),
(N'customer2', N'sPwWfpRHa3gJT8ulcuwlO2Cg2UajK2uCFfETiAxJpzc=', N'salt_cust2_2026', 2, 1, GETDATE());
GO

-- 3.3. Dữ liệu bảng Customer (Gắn với tài khoản customer1 và customer2)
INSERT INTO Customer (UserID, FullName, Phone, Email, Address)
VALUES
(2, N'Nguyễn Văn An', N'0901234567', N'customer1@gmail.com', N'Số 123 Đường Nguyễn Trãi, Quận 1, TP. Hồ Chí Minh'),
(3, N'Trần Thị Bích', N'0987654321', N'customer2@gmail.com', N'Tòa nhà Landmark 81, Quận Bình Thạnh, TP. Hồ Chí Minh');
GO

-- 3.4. Dữ liệu bảng Category (4 danh mục thời trang chính)
INSERT INTO Category (CategoryName)
VALUES
(N'Áo thời trang'),
(N'Quần & Chân váy'),
(N'Đầm & Đồ bộ'),
(N'Phụ kiện thời trang');
GO

-- 3.5. Dữ liệu bảng Product (16 sản phẩm thời trang mẫu img-01.jpg -> img-16.jpg)
-- ĐẶC BIỆT: Sản phẩm số 8 có StockQuantity = 0 để demo nghiệp vụ Hết hàng.
INSERT INTO Product (ProductName, [Description], CategoryID, Price, ImageUrl, StockQuantity, IsActive)
VALUES
-- Danh mục 1: Áo thời trang (CategoryID = 1)
(N'Áo Sơ Mi Lụa Cổ V Thanh Lịch', 
 N'Chất liệu lụa satin cao cấp mềm mịn, thoáng mát, form dáng suông công sở hiện đại.', 
 1, 350000, N'img-01.jpg', 25, 1),

(N'Áo Thun Cotton Unisex Oversize', 
 N'Chất liệu 100% Cotton 2 chiều co giãn tốt, thấm hút mồ hôi, phong cách đường phố năng động.', 
 1, 189000, N'img-02.jpg', 50, 1),

(N'Áo Blazer Nữ Dáng Suông Hàn Quốc', 
 N'Áo khoác Blazer 2 lớp vải tuyết mưa đứng form, chuẩn phong cách quý cô công sở thanh lịch.', 
 1, 520000, N'img-03.jpg', 15, 1),

(N'Áo Len Dệt Kim Cổ Lọ Thu Đông', 
 N'Sợi len tăm dệt kim giữ ấm tối ưu, co giãn 4 chiều mềm mại ôm nhẹ vóc dáng.', 
 1, 260000, N'img-04.jpg', 30, 1),

-- Danh mục 2: Quần & Chân váy (CategoryID = 2)
(N'Quần Jeans Nữ Ống Đứng Lưng Cao', 
 N'Vải bò denim cotton dày dặn, tôn dáng, thiết kế cạp cao hack dáng cực chuẩn.', 
 2, 380000, N'img-05.jpg', 20, 1),

(N'Chân Váy Xếp Ly Tennis Năng Động', 
 N'Chân váy chữ A xếp ly đều, kèm quần bảo hộ bên trong tiện lợi cho mọi hoạt động.', 
 2, 220000, N'img-06.jpg', 18, 1),

(N'Quần Tây Nữ Ống Rộng Suông Dài', 
 N'Vải đũi tuyết hàn không nhăn, cạp khóa thời trang, tôn đôi chân dài thon gọn.', 
 2, 340000, N'img-07.jpg', 22, 1),

(N'Chân Váy Midi Hoa Nhí Dáng Xòe', 
 N'Vải voan tơ 2 lớp bay bổng, họa tiết hoa nhí vintage dịu dàng và cuốn hút (Sản phẩm demo hết hàng).', 
 2, 290000, N'img-08.jpg', 0, 1), -- HẾT HÀNG (StockQuantity = 0) ĐỂ DEMO NGHIỆP VỤ

-- Danh mục 3: Đầm & Đồ bộ (CategoryID = 3)
(N'Đầm Suông Linen Phối Cúc Gỗ Cổ Điển', 
 N'Vải linen tự nhiên thân thiện với làn da, thiết kế tối giản mang lại sự thoải mái tuyệt đối.', 
 3, 450000, N'img-09.jpg', 12, 1),

(N'Đầm Dạ Hội Cổ Yếm Xẻ Tà Quyến Rũ', 
 N'Thiết kế ôm body xẻ tà sang trọng, chất liệu thun dạ kim tuyến lấp lánh dự tiệc tối.', 
 3, 680000, N'img-10.jpg', 8, 1),

(N'Set Bộ Áo Vest Cộc Kèm Chân Váy', 
 N'Thiết kế set đồ ton-sur-ton thời thượng, tone màu pastel nhã nhặn dễ phối đồ.', 
 3, 590000, N'img-11.jpg', 14, 1),

(N'Đầm Xòe Hoa Nhí Cổ Vuông Tay Bồng', 
 N'Cổ vuông tôn xương quai xanh quyến rũ, tay phồng công chúa ngọt ngào trẻ trung.', 
 3, 420000, N'img-12.jpg', 16, 1),

-- Danh mục 4: Phụ kiện thời trang (CategoryID = 4)
(N'Túi Xách Nữ Da PU Kẹp Nách Baguette', 
 N'Da PU cao cấp chống xước, khóa kéo trơn tru, kiểu dáng kẹp nách xu hướng Y2K.', 
 4, 320000, N'img-13.jpg', 25, 1),

(N'Thắt Lưng Nữ Da Thật Mặt Khóa Kim Loại', 
 N'Dây da bản nhỏ 1.8cm tinh tế, phối hợp hoàn hảo với quần tây hoặc thắt eo váy.', 
 4, 120000, N'img-14.jpg', 40, 1),

(N'Mũ Nồi Beret Len Dạ Phong Cách Pháp', 
 N'Chất dạ nỉ dày dặn ấm áp, phụ kiện hoàn hảo cho những set đồ mùa thu đông lãng mạn.', 
 4, 150000, N'img-15.jpg', 30, 1),

(N'Khăn Lụa Vuông Họa Tiết Baroque Sang Trọng', 
 N'Kích thước 70x70cm, chất lụa tơ tằm mềm mại, dùng quàng cổ hoặc phối túi xách.', 
 4, 95000, N'img-16.jpg', 35, 1);
GO

-- 3.6. Dữ liệu mẫu đơn hàng (OrderPro & OrderDetail) để kiểm thử lịch sử đơn
-- Đơn hàng 1: Khách 1 đã Hoàn thành (Completed)
INSERT INTO OrderPro (OrderDate, CustomerID, ShippingAddress, TotalAmount, [Status])
VALUES (DATEADD(DAY, -5, GETDATE()), 1, N'Số 123 Đường Nguyễn Trãi, Quận 1, TP. Hồ Chí Minh', 700000, N'Completed');

DECLARE @Order1ID INT = SCOPE_IDENTITY();
INSERT INTO OrderDetail (OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(@Order1ID, 1, 2, 350000);

-- Đơn hàng 2: Khách 1 đang Đang giao (Shipping)
INSERT INTO OrderPro (OrderDate, CustomerID, ShippingAddress, TotalAmount, [Status])
VALUES (DATEADD(DAY, -2, GETDATE()), 1, N'Số 123 Đường Nguyễn Trãi, Quận 1, TP. Hồ Chí Minh', 520000, N'Shipping');

DECLARE @Order2ID INT = SCOPE_IDENTITY();
INSERT INTO OrderDetail (OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(@Order2ID, 3, 1, 520000);

-- Đơn hàng 3: Khách 2 đang Chờ xác nhận (Pending)
INSERT INTO OrderPro (OrderDate, CustomerID, ShippingAddress, TotalAmount, [Status])
VALUES (DATEADD(HOUR, -3, GETDATE()), 2, N'Tòa nhà Landmark 81, Quận Bình Thạnh, TP. Hồ Chí Minh', 809000, N'Pending');

DECLARE @Order3ID INT = SCOPE_IDENTITY();
INSERT INTO OrderDetail (OrderID, ProductID, Quantity, UnitPrice)
VALUES 
(@Order3ID, 2, 1, 189000),
(@Order3ID, 13, 1, 320000),
(@Order3ID, 9, 1, 300000);
GO

PRINT N'======================================================';
PRINT N'ĐÃ TẠO DATABASE VÀ CHÈN DỮ LIỆU MẪU THÀNH CÔNG!';
PRINT N'Tài khoản đăng nhập kiểm thử:';
PRINT N'  1. Admin:     admin / Admin@123';
PRINT N'  2. Khách 1:   customer1 / Customer@123';
PRINT N'  3. Khách 2:   customer2 / Customer@123';
PRINT N'Sản phẩm demo hết hàng: ProductID = 8 (StockQuantity = 0)';
PRINT N'======================================================';
GO
