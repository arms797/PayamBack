using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity.Permission
{
    public class PermissionCreateDto
    {
        [Required(ErrorMessage = "منبع الزامی است")]
        [MaxLength(100)]
        public string Resource { get; set; } = string.Empty;

        [Required(ErrorMessage = "عملیات الزامی است")]
        [MaxLength(50)]
        public string Action { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام مجوز الزامی است")]
        [MaxLength(150)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}