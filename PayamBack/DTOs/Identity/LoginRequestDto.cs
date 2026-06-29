using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity
{
    public class LoginRequestDto
    {
        [Required(ErrorMessage = "نام کاربری الزامی است")]
        [MaxLength(100)]
        public string Username { get; set; } = string.Empty;

        [Required(ErrorMessage = "رمز عبور الزامی است")]
        [MaxLength(100)]
        public string Password { get; set; } = string.Empty;

        public string? CaptchaKey { get; set; }
        public string? CaptchaAnswer { get; set; }
    }
}