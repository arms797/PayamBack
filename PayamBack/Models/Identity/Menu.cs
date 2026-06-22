using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    public class Menu
    {
        [Key]
        public int Id { get; set; }

        // شناسه منوی والد (برای ساخت منوی چندسطحی)
        public int? ParentId { get; set; }

        // عنوان نمایشی منو، مثلاً "مدیریت اساتید"
        [MaxLength(100)]
        public string? Title { get; set; }

        // نام آیکون برای فرانت‌اند، مثلاً "FaUser"
        [MaxLength(50)]
        public string? Icon { get; set; }

        // آدرس صفحه در فرانت‌اند، مثلاً "/ostad/list"
        [MaxLength(200)]
        public string? Path { get; set; }

        // نام مجوز مورد نیاز برای دیدن این منو (ارتباط با Permission.Name)
        [MaxLength(150)]
        public string? PermissionName { get; set; }

        // ترتیب نمایش منوها
        public int? Order { get; set; }

        // فعال/غیرفعال
        public bool? Vazeeat { get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ======== Navigation Properties (ساختار درختی) ========
        [ForeignKey(nameof(ParentId))]
        public virtual Menu? Parent { get; set; }

        public virtual ICollection<Menu>? Children { get; set; }
    }
}