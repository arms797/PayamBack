using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity.Permission
{
    public class PermissionUpdateDto
    {
        [MaxLength(100)]
        public string? Resource { get; set; }

        [MaxLength(50)]
        public string? Action { get; set; }

        [MaxLength(150)]
        public string? Name { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool? IsActive { get; set; }
    }
}