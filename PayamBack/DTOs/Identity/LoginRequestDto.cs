using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity
{
    /// <summary>
    /// DTO برای دریافت اطلاعات ورود از کاربر
    /// </summary>
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;
    }
}