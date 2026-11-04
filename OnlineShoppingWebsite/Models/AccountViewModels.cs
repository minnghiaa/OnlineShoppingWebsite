using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OnlineShoppingWebsite.Models
{
    // =========================================================
    // VIEWMODEL DÙNG CHO FORM SIGN UP
    // =========================================================

    // SignUpViewModel không phải bảng CSDL.
    // Nó chỉ gom các dữ liệu người dùng nhập trên form Đăng ký.
    public class SignUpViewModel
    {
        // Bắt buộc nhập UserName.
        [Required(
            ErrorMessage = "Vui lòng nhập tên đăng nhập.")]

        // UserAccount.UserName trong CSDL có độ dài tối đa 50.
        [StringLength(50)]

        // Tên hiển thị trên Label.
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }


        // Bắt buộc nhập Password.
        [Required(
            ErrorMessage = "Vui lòng nhập mật khẩu.")]

        // Trong bài này yêu cầu ít nhất 6 ký tự.
        [StringLength(
            50,
            MinimumLength = 6,
            ErrorMessage = "Mật khẩu phải có ít nhất 6 ký tự.")]

        // View sẽ hiển thị ô nhập dạng Password.
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }


        // Người dùng phải nhập lại Password.
        [Required(
            ErrorMessage = "Vui lòng nhập lại mật khẩu.")]

        [DataType(DataType.Password)]

        // Compare kiểm tra ConfirmPassword
        // phải giống với property Password ở phía trên.
        [Compare(
            "Password",
            ErrorMessage = "Mật khẩu nhập lại không khớp.")]

        [Display(Name = "Nhập lại mật khẩu")]
        public string ConfirmPassword { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập họ tên.")]
        [StringLength(100)]
        [Display(Name = "Họ tên")]
        public string FullName { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập số điện thoại.")]
        [StringLength(20)]
        [Display(Name = "Số điện thoại")]
        public string Phone { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập email.")]

        // Kiểm tra định dạng email cơ bản.
        [EmailAddress(
            ErrorMessage = "Email không đúng định dạng.")]

        [StringLength(100)]
        [Display(Name = "Email")]
        public string Email { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập địa chỉ.")]
        [StringLength(250)]
        [Display(Name = "Địa chỉ")]
        public string Address { get; set; }
    }

    // =========================================================
    // VIEWMODEL DÙNG CHO FORM LOGIN
    // =========================================================

    public class LoginViewModel
    {
        [Required(
            ErrorMessage = "Vui lòng nhập tên đăng nhập.")]
        [Display(Name = "Tên đăng nhập")]
        public string UserName { get; set; }


        [Required(
            ErrorMessage = "Vui lòng nhập mật khẩu.")]
        [DataType(DataType.Password)]
        [Display(Name = "Mật khẩu")]
        public string Password { get; set; }
    }


}