using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity.Role
{
    public class RoleCreateDto
    {
        [Required(ErrorMessage = "نام نقش الزامی است")]
        [MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد نقش الزامی است")]
        public int CodeRole { get; set; }

        public bool? Vazeeyat { get; set; }
        public bool? Emza { get; set; }
        public bool? IsAdmin { get; set; }  // ← اضافه شد
    }
}