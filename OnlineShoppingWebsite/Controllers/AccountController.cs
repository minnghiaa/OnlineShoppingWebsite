using System;
using System.Data.Entity;
using System.Linq;
using System.Web.Mvc;
using OnlineShoppingWebsite.Models;

namespace OnlineShoppingWebsite.Controllers
{
    // =========================================================
    // ACCOUNT CONTROLLER (BUỔI 08 - SIGN UP, LOGIN, LOGOUT)
    // =========================================================
    public class AccountController : Controller
    {
        // Kết nối CSDL OnlineShoppingDB bằng EF Database First.
        private OnlineShoppingEntities db = new OnlineShoppingEntities();

        // GET: Account
        public ActionResult Index()
        {
            return RedirectToAction("Login");
        }

        // =====================================================
        // CHỨC NĂNG 1: MỞ FORM SIGN UP
        // =====================================================
        [HttpGet]
        public ActionResult SignUp()
        {
            return View();
        }

        // =====================================================
        // CHỨC NĂNG 2: SIGN UP - TẠO USERACCOUNT VÀ CUSTOMER
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult SignUp(SignUpViewModel model)
        {
            // Dữ liệu form phải hợp lệ trước khi xử lý CSDL.
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. KIỂM TRA USERNAME
            bool userNameExists = db.UserAccounts.Any(u => u.UserName == model.UserName);
            if (userNameExists)
            {
                ModelState.AddModelError("UserName", "Tên đăng nhập đã tồn tại.");
                return View(model);
            }

            // 2. LẤY ROLE CUSTOMER
            // Tài khoản đăng ký công khai luôn là Customer.
            var customerRole = db.Roles.FirstOrDefault(r => r.RoleName == "Customer");
            if (customerRole == null)
            {
                ModelState.AddModelError("", "Không tìm thấy Role Customer trong hệ thống.");
                return View(model);
            }

            // 3. TẠO SALT + PASSWORD HASH (PBKDF2)
            string salt = PasswordHelper.CreateSalt();
            string passwordHash = PasswordHelper.Hash(model.Password, salt);

            // 4. BẮT ĐẦU TRANSACTION
            using (var transaction = db.Database.BeginTransaction())
            {
                try
                {
                    // 4.1. Tạo UserAccount
                    var account = new UserAccount
                    {
                        UserName = model.UserName.Trim(),
                        PasswordHash = passwordHash,
                        PasswordSalt = salt,
                        RoleID = customerRole.RoleID,
                        IsActive = true,
                        CreatedAt = DateTime.Now
                    };
                    db.UserAccounts.Add(account);
                    db.SaveChanges();

                    // 4.2. Tạo Customer gắn với UserID vừa sinh
                    var customer = new Customer
                    {
                        UserID = account.UserID,
                        FullName = model.FullName.Trim(),
                        Phone = model.Phone != null ? model.Phone.Trim() : null,
                        Email = model.Email != null ? model.Email.Trim() : null,
                        Address = model.Address != null ? model.Address.Trim() : null
                    };
                    db.Customers.Add(customer);
                    db.SaveChanges();

                    transaction.Commit();

                    TempData["SignUpSuccess"] = "Đăng ký tài khoản thành công! Vui lòng đăng nhập để tiếp tục.";
                    return RedirectToAction("Login");
                }
                catch (Exception ex)
                {
                    transaction.Rollback();
                    ModelState.AddModelError("", "Có lỗi khi tạo tài khoản: " + ex.GetBaseException().Message + ". Vui lòng thử lại!");
                    return View(model);
                }
            }
        }

        // =====================================================
        // CHỨC NĂNG 3: MỞ FORM LOGIN
        // =====================================================
        [HttpGet]
        public ActionResult Login()
        {
            return View();
        }

        // =====================================================
        // CHỨC NĂNG 3 & 6: LOGIN VÀ LƯU SESSION
        // =====================================================
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Login(LoginViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // 1. TÌM USERACCOUNT THEO USERNAME
            var account = db.UserAccounts
                .Include(u => u.Role)
                .FirstOrDefault(u => u.UserName == model.UserName && u.IsActive);

            if (account == null)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 2. KIỂM TRA MẬT KHẨU BẰNG PBKDF2 HASH + SALT
            bool passwordCorrect = PasswordHelper.Verify(
                model.Password,
                account.PasswordSalt,
                account.PasswordHash);

            if (!passwordCorrect)
            {
                ModelState.AddModelError("", "Tên đăng nhập hoặc mật khẩu không đúng.");
                return View(model);
            }

            // 3. LƯU TRẠNG THÁI LOGIN VÀO SESSION
            Session["UserID"] = account.UserID;
            Session["UserName"] = account.UserName;
            Session["Role"] = account.Role.RoleName;
            Session["RoleName"] = account.Role.RoleName;

            // 4. NẾU LÀ CUSTOMER -> LẤY HỒ SƠ CUSTOMER TƯƠNG ỨNG
            var customer = db.Customers.FirstOrDefault(c => c.UserID == account.UserID);
            if (customer != null)
            {
                Session["CustomerID"] = customer.CustomerID;
                Session["FullName"] = customer.FullName;
            }
            else
            {
                Session.Remove("CustomerID");
                Session.Remove("FullName");
            }

            // 5. CHUYỂN TRANG SAU KHI LOGIN
            // Cả Customer và Admin đều quay về Trang Chủ Store,
            // Header sẽ đọc Session["Role"] để hiển thị menu phù hợp.
            return RedirectToAction("Index", "Store");
        }

        // =====================================================
        // CHỨC NĂNG 4: LOGOUT
        // =====================================================
        // Dùng POST để bảo mật phiên đăng nhập, tránh tấn công CSRF.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public ActionResult Logout()
        {
            // Chỉ xóa các Session liên quan tài khoản, KHÔNG xóa Session["Cart"]
            Session.Remove("UserID");
            Session.Remove("UserName");
            Session.Remove("Role");
            Session.Remove("RoleName");
            Session.Remove("CustomerID");
            Session.Remove("FullName");

            return RedirectToAction("Index", "Store");
        }

        // Giải phóng DbContext khi Controller kết thúc
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
