using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;

namespace PayamBack.Models.Schedule
{
    /// <summary>
    /// جدول ارائه دروس (گروه‌های درسی هر ترم)
    /// هر رکورد = یه گروه درسی از یه درس، در یه رشته، در یه مرکز، در یه ترم
    /// </summary>
    [Table("DarsEraeh")]
    public class DarsEraeh
    {
        // ============================================================
        // کلید اصلی
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // روابط اصلی
        // ============================================================
        /// <summary>کد ترم</summary>
        [Required(ErrorMessage = "ترم الزامی است")]
        [StringLength(20)]
        public string CodeTerm { get; set; } = string.Empty;

        /// <summary>شناسه مرکز (FK به Markaz)</summary>
        [Required(ErrorMessage = "مرکز الزامی است")]
        public int MarkazId { get; set; }

        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }

        /// <summary>شناسه رشته (FK به Rashteh)</summary>
        [Required(ErrorMessage = "رشته الزامی است")]
        public int ReshtehId { get; set; }

        [ForeignKey(nameof(ReshtehId))]
        public virtual Reshteh? Reshteh { get; set; }

        /// <summary>شناسه درس (FK به Dars)</summary>
        [Required(ErrorMessage = "درس الزامی است")]
        public int DarsId { get; set; }

        [ForeignKey(nameof(DarsId))]
        public virtual Dars? Dars { get; set; }
        [Required]
        [StringLength(20)]
        public string CodeDars { get; set; } = string.Empty;

        // ============================================================
        // اطلاعات گروه درسی
        // ============================================================
        /// <summary>شماره گروه</summary>
        [Required(ErrorMessage = "شماره گروه الزامی است")]
        public int Grooh { get; set; }

        /// <summary>نوع تدریس</summary>
        [StringLength(100)]
        public string? NoeTadris { get; set; }

        // ============================================================
        // مجوز
        // ============================================================
        /// <summary>شناسه کاربر مجوز دهنده</summary>
        public int? UserIdMojavezDahandeh { get; set; }

        /// <summary>نقش کاربر مجوز دهنده</summary>
        [StringLength(100)]
        public string? NaghshMarkazMojavezDahandeh { get; set; }

        // ============================================================
        // زمان ارائه
        // ============================================================
        /// <summary>تاریخ و ساعت ارائه درس</summary>
        public DateTime? TarikheEraheh { get; set; }

        // ============================================================
        // مشخصات اخذ
        // ============================================================
        /// <summary>جنسیت مجاز برای اخذ</summary>
        public int? Jensiat { get; set; }

        /// <summary>وضعیت درس</summary>
        public bool VazeeyatDars { get; set; } = true;

        /// <summary>نحوه ارائه درس</summary>
        public int? NahvehEraehDars { get; set; }

        /// <summary>امکان اخذ توسط سایر مراکز</summary>
        public bool EmkanAkhzSayerMarakez { get; set; } = false;

        /// <summary>امکان لینک به درس اصلی مرکز دیگر</summary>
        public bool EmkanLinkBeSayerMarakez { get; set; } = false;

        // ============================================================
        // 🔥 ارتباط دهی (برنامه مشترک) - خودارجاعی
        // ============================================================
        /// <summary>
        /// شناسه گروه اصلی
        /// - null: این گروه اصلیه و خودش برنامه داره
        /// - مقدار: فرعی‌ست و از برنامه گروه اصلی استفاده می‌کنه
        /// </summary>
        public int? ErtebatDehiId { get; set; }

        [ForeignKey(nameof(ErtebatDehiId))]
        public virtual DarsEraeh? DarsEraehAsli { get; set; }

        /// <summary>گروه‌های فرعی که از این گروه برنامه می‌گیرن</summary>
        public virtual ICollection<DarsEraeh>? ZirMajmooeh { get; set; }
        /// <summary>استادهای این درس ارائه شده</summary>
        public virtual ICollection<DarsEraehOstad>? Ostads { get; set; }

        // ============================================================
        // برنامه‌ریزی
        // ============================================================
        /// <summary>وضعیت برنامه‌ریزی</summary>
        [StringLength(100)]
        public string? BarnamehRizi { get; set; }

        // ============================================================
        // آمار
        // ============================================================
        /// <summary>تعداد دانشجوی ثبت‌نام شده</summary>
        public int? SabtNami { get; set; }

        // ============================================================
        // فیلدهای سیستمی
        // ============================================================
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        

        // 🔥 جلسات این گروه درسی (بعداً اضافه میشه)
        // public virtual ICollection<Jalaseh>? JalasehList { get; set; }
    }
}