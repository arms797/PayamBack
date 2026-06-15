using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    public class AppUserRole : IdentityUserRole<int>
    {
        // کلید خارجی به جدول مرکز (Markaz)
        [Required]
        public int MarkazId { get; set; }

        // نقش پیش‌فرض این کاربر در مرکز مربوطه
        [Required]
        public bool RolePishFarz { get; set; } = false;

        // دسترسی به اطلاعات مرکز
        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }
    }
}