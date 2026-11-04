using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using System.Security.Cryptography;

namespace OnlineShoppingWebsite.Models
{
    // =========================================================
    // HELPER XỬ LÝ PASSWORD CHO BÀI THỰC HÀNH
    // =========================================================

    public static class PasswordHelper
    {
        // =====================================================
        // TẠO SALT NGẪU NHIÊN
        // =====================================================

        public static string CreateSalt()
        {
            // Tạo mảng 16 byte.
            // Mỗi tài khoản sẽ có Salt khác nhau.
            byte[] saltBytes =
                new byte[16];


            // RandomNumberGenerator tạo dữ liệu ngẫu nhiên
            // phù hợp hơn Random thông thường cho mục đích bảo mật.
            using (var rng =
                RandomNumberGenerator.Create())
            {
                // Ghi dữ liệu ngẫu nhiên vào saltBytes.
                rng.GetBytes(saltBytes);
            }


            // CSDL lưu chuỗi, nên chuyển byte[]
            // thành chuỗi Base64 trước khi trả về.
            return Convert.ToBase64String(
                saltBytes);
        }


        // =====================================================
        // HASH PASSWORD
        // =====================================================

        public static string Hash(
            string password,
            string salt)
        {
            // Chuyển Salt từ chuỗi Base64 trong CSDL
            // trở lại thành byte[].
            byte[] saltBytes =
                Convert.FromBase64String(salt);


            // Rfc2898DeriveBytes triển khai PBKDF2.
            //
            // password:
            // mật khẩu người dùng nhập.
            //
            // saltBytes:
            // Salt riêng của tài khoản.
            //
            // 100000:
            // số vòng lặp dùng để làm việc dò mật khẩu chậm hơn.
            using (var pbkdf2 =
                new Rfc2898DeriveBytes(
                    password,
                    saltBytes,
                    100000))
            {
                // Lấy 32 byte kết quả hash.
                byte[] hashBytes =
                    pbkdf2.GetBytes(32);


                // Chuyển hash thành Base64
                // để lưu vào UserAccount.PasswordHash.
                return Convert.ToBase64String(
                    hashBytes);
            }
        }


        // =====================================================
        // KIỂM TRA PASSWORD KHI LOGIN
        // =====================================================

        public static bool Verify(
            string password,
            string salt,
            string storedHash)
        {
            // Hash lại Password người dùng vừa nhập
            // bằng đúng Salt đã lưu trong CSDL.
            string newHash =
                Hash(password, salt);


            // Hai chuỗi hash giống nhau
            // nghĩa là Password nhập vào đúng.
            return newHash == storedHash;
        }
    }

}
