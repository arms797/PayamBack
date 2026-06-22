using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    public class Permission
    {
        [Key]
        public int Id { get; set; }

        // نام منبع (همان نام کنترلر)، مثلاً "Ostad"
        [MaxLength(100)]
        public string? Resource { get; set; }

        // نام عملیات (همان نام اکشن)، مثلاً "Create"
        [MaxLength(50)]
        public string? Action { get; set; }

        // نام ترکیبی (Resource + "." + Action) مثلاً "Ostad.Create"
        [MaxLength(150)]
        public string? Name { get; set; }

        // توضیحات برای مدیریت بهتر
        [MaxLength(500)]
        public string? Description { get; set; }

        // فعال/غیرفعال
        public bool? IsActive { get; set; }

        // تاریخ ایجاد
        public DateTime? CreatedAt { get; set; }

        // ارتباط با RolePermissions (یک Permission در چند نقش می‌تواند باشد)
        public virtual ICollection<RolePermission>? RolePermissions { get; set; }
    }
}