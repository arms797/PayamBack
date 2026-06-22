using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity
{
    /// <summary>
    /// DTO برای تمدید AccessToken با RefreshToken
    /// </summary>
    public class RefreshTokenRequestDto
    {
        [Required]
        public string AccessToken { get; set; } = string.Empty;

        [Required]
        public string RefreshToken { get; set; } = string.Empty;
    }
}