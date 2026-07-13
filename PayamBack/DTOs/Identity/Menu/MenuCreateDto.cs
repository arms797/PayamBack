using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity.Menu
{
    public class MenuCreateDto
    {
        [Required(ErrorMessage = "عنوان منو الزامی است")]
        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;

        public int? ParentId { get; set; }

        [MaxLength(200)]
        public string? Path { get; set; }

        [MaxLength(50)]
        public string? Icon { get; set; }

        [MaxLength(150)]
        public string? PermissionName { get; set; }

        public int? Order { get; set; }

        public bool? Vazeeat { get; set; }
    }
}