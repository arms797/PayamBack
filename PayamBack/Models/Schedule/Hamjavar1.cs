using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Models.Schedule
{
    /// <summary>
    /// جزئیات تکمیلی درخواست هم‌جاوری
    /// </summary>
    public class Hamjavar1
    {
        [Key]
        public int Id { get; set; }

        // ============================================================
        // کلید خارجی به Hamjavar
        // ============================================================
        public int HamjavarId { get; set; }

        // ============================================================
        // اطلاعات ثبت‌کننده
        // ============================================================
        /// <summary>شناسه کاربر ثبت‌کننده</summary>
        public int? UserIdSabtKonandeh { get; set; }

        /// <summary>شناسه نقش ثبت‌کننده</summary>
        public string? RoleMarkazSabtKonandeh { get; set; }

        /// <summary>شناسه مرکز</summary>
        public int? MarkazId { get; set; }

        /// <summary>داخل/خارج استان (true = داخل استان)</summary>
        public bool? InOstan { get; set; }

        /// <summary>
        /// شناسه فعالیت‌ها با جداکننده '|' 
        /// مثال: "1|3|5" یعنی فعالیت‌های با Id 1, 3, 5
        /// </summary>
        [MaxLength(50)]
        public string? FaaliatIds { get; set; }

        // ============================================================
        // تعداد روزهای بررسی در هر مرحله
        // ============================================================
        /// <summary>تعداد روز تقاضای عضو علمی</summary>
        public int? TedadRoozElmi { get; set; }

        /// <summary>تعداد روز نظر رئیس مرکز</summary>
        public int? TedadRoozRaeis { get; set; }

        /// <summary>تعداد روز نظر مدیر خدمات آموزشی استان</summary>
        public int? TedadRoozKhadamat { get; set; }

        /// <summary>تعداد روز نظر معاونت آموزشی استان</summary>
        public int? TedadRoozMoaven { get; set; }

        // ============================================================
        // Navigation Properties
        // ============================================================
        [ForeignKey(nameof(HamjavarId))]
        public virtual Hamjavar? Hamjavar { get; set; }

        [ForeignKey(nameof(UserIdSabtKonandeh))]
        public virtual AppUser? UserSabtKonandeh { get; set; }

        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }
    }
}