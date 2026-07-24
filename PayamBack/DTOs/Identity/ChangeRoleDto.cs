using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity
{
    /// <summary>
    /// DTO برای تغییر نقش فعال کاربر
    /// </summary>
    public class ChangeRoleDto
    {
        [Required]
        public int RoleId { get; set; }
        public int? MarkazId {  get; set; }
    }
}