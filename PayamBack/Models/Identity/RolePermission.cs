using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    public class RolePermission
    {
        [Key]
        public int Id { get; set; }

        // شناسه نقش (کلید خارجی به جدول AspNetRoles)
        public int? RoleId { get; set; }

        // شناسه مجوز (کلید خارجی به جدول Permissions)
        public int? PermissionId { get; set; }

        // فعال/غیرفعال بودن این مجوز برای این نقش
        public bool? Vazeeat { get; set; }

        // ======== Navigation Properties ========
        [ForeignKey(nameof(RoleId))]
        public virtual AppRole? Role { get; set; }

        [ForeignKey(nameof(PermissionId))]
        public virtual Permission? Permission { get; set; }
    }
}