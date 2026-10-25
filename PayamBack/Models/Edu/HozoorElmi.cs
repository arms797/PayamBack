using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// مدل حضور علمی (Hozoor Elmi).
    /// شامل اطلاعات ورود/خروج استاد، ثبت کننده‌ها، نظرات علمی و وضعیت قفل.
    /// </summary>
    public class HozoorElmi
    {
        [Key]
        public int Id { get; set; }

        // ============================================================
        // استاد و مرکز
        // ============================================================
        /// <summary>
        /// آی‌دی استاد (FK به Ostad)
        /// </summary>
        public int OstadId { get; set; }

        /// <summary>
        /// آی‌دی مرکز (FK به Markaz)
        /// </summary>
        public int MarkazId { get; set; }

        // ============================================================
        // کاربران ثبت کننده
        // ============================================================
        /// <summary>
        /// آی‌دی کاربری که ورود را ثبت کرده
        /// </summary>
        public int? UserIdIn { get; set; }

        /// <summary>
        /// آی‌دی کاربری که خروج را ثبت کرده
        /// </summary>
        public int? UserIdOut { get; set; }

        // ============================================================
        // نقش مرکز ثبت کننده
        // ============================================================
        /// <summary>
        /// نقش مرکز ثبت‌کننده ورود
        /// </summary>
        [MaxLength(200)]
        public string? RoleMarkazIn { get; set; }

        /// <summary>
        /// نقش مرکز ثبت‌کننده خروج
        /// </summary>
        [MaxLength(200)]
        public string? RoleMarkazOut { get; set; }

        // ============================================================
        // تاریخ و ساعت ورود/خروج
        // ============================================================
        /// <summary>
        /// تاریخ و ساعت واقعی ورود
        /// </summary>
        public DateTime? TarikhIn { get; set; }

        /// <summary>
        /// تاریخ و ساعت واقعی خروج
        /// </summary>
        public DateTime? TarikhOut { get; set; }

        /// <summary>
        /// تاریخ و ساعت ثبت ورود در سیستم
        /// </summary>
        public DateTime? TarikhSabtIn { get; set; }

        /// <summary>
        /// تاریخ و ساعت ثبت خروج در سیستم
        /// </summary>
        public DateTime? TarikhSabtOut { get; set; }

        // ============================================================
        // توضیحات
        // ============================================================
        /// <summary>
        /// توضیحات ثبت‌کننده
        /// </summary>
        [MaxLength(2000)]
        public string? TozihatSabtKonandeh { get; set; }

        /// <summary>
        /// توضیحات علمی
        /// </summary>
        [MaxLength(2000)]
        public string? TozihatElmi { get; set; }

        // ============================================================
        // مدرک و نظر علمی
        // ============================================================
        /// <summary>
        /// بارگذاری مدرک (آدرس فایل یا Base64)
        /// </summary>
        [MaxLength(500)]
        public string? Madrak { get; set; }

        /// <summary>
        /// تاریخ و ساعت نظر علمی
        /// </summary>
        public DateTime? TarikhElmi { get; set; }

        /// <summary>
        /// نظر نهایی علمی
        /// </summary>
        public bool RecordAsli { get; set; } = false;

        // ============================================================
        // قفل و وضعیت حضور
        // ============================================================
        /// <summary>
        /// وضعیت قفل بودن رکورد
        /// </summary>
        public bool IsLock { get; set; } = false;

        /// <summary>
        /// توضیحات قفل
        /// </summary>
        [MaxLength(1000)]
        public string? TozihatLock { get; set; }

        /// <summary>
        /// وضعیت حضور (1 = حاضر، 0 = غایب یا مقادیر دیگر طبق منطق)
        /// </summary>
        public int? Hozoor { get; set; } = 1;

        [ForeignKey(nameof(OstadId))]
        public Ostad? Ostad { get; set; }

        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }

        [ForeignKey(nameof(UserIdIn))]
        public AppUser? UserIn { get; set; }
        [ForeignKey(nameof(UserIdOut))]
        public AppUser? UserOut { get; set; }

    }
}