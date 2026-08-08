using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarStatusDto
    {
        [Required]
        public int Id { get; set; }

        [Required]
        public string Status { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Tozihat { get; set; }
    }
}