using Microsoft.AspNetCore.Identity;
using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    [Table("AspNetUserRoles")]
    public class AppUserRole : IdentityUserRole<int>
    {
        /// <summary>کلید اصلی جدید (جایگزین کلید مرکب Identity)</summary>
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // 🔥 بازتعریف صریح UserId و RoleId برای اعمالForeignKey
        // ============================================================

        /// <summary>شناسه کاربر</summary>
        public override int UserId { get; set; }

        /// <summary>شناسه نقش</summary>
        public override int RoleId { get; set; }

        // ============================================================
        // فیلدهای اضافی
        // ============================================================

        /// <summary>کلید خارجی به جدول مرکز (Markaz)</summary>
        public int? MarkazId { get; set; }

        /// <summary>نقش پیش‌فرض این کاربر در مرکز مربوطه</summary>
        public bool? RolePishFarz { get; set; }

        /// <summary>
        /// شناسه رکورد والد در همین جدول (خود ارجاعی)
        /// یعنی این کاربر زیر نظر کدام کاربر دیگر در این مرکز فعالیت می‌کند.
        /// </summary>
        public int? ParentUserRoleId { get; set; }

        // ============================================================
        // 🔥 Navigation Properties (برای دسترسی کامل به اشیاء مرتبط)
        // ============================================================

        /// <summary>کاربر مرتبط با این نقش در این مرکز</summary>
        [ForeignKey(nameof(UserId))]
        public virtual AppUser? User { get; set; }

        /// <summary>نقش مرتبط با این کاربر در این مرکز</summary>
        [ForeignKey(nameof(RoleId))]
        public virtual AppRole? Role { get; set; }

        /// <summary>مرکز مرتبط با این کاربر و نقش</summary>
        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }

        /// <summary>رکورد والد (مدیر بالادستی)</summary>
        [ForeignKey(nameof(ParentUserRoleId))]
        public virtual AppUserRole? ParentUserRole { get; set; }

        /// <summary>زیردستان این رکورد (کاربرانی که زیر نظر این کاربر هستند)</summary>
        public virtual ICollection<AppUserRole>? ChildUserRoles { get; set; }
    }
}