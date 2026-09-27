using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// جدول ساختمان‌ها و کلاس‌ها
    /// هر رکورد = یک کلاس فیزیکی در یک ساختمان
    /// </summary>
    [Table("SakhtemanKelass")]
    public class SakhtemanKelass
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
        /// <summary>شناسه مرکز (FK به Markaz)</summary>
        [Required(ErrorMessage = "مرکز الزامی است")]
        public int MarkazId { get; set; }

        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }

        // ============================================================
        // اطلاعات ساختمان
        // ============================================================
        /// <summary>کد ساختمان</summary>
        [Required(ErrorMessage = "کد ساختمان الزامی است")]
        public int CodeSakhteman { get; set; }

        /// <summary>نام ساختمان</summary>
        [Required(ErrorMessage = "نام ساختمان الزامی است")]
        [StringLength(100)]
        public string NaamSakhteman { get; set; } = string.Empty;

        // ============================================================
        // اطلاعات کلاس
        // ============================================================
        /// <summary>کد کلاس</summary>
        [Required(ErrorMessage = "کد کلاس الزامی است")]
        public int CodeClass { get; set; }

        /// <summary>نام کلاس</summary>
        [Required(ErrorMessage = "نام کلاس الزامی است")]
        [StringLength(100)]
        public string NammClass { get; set; } = string.Empty;

        /// <summary>نوع کلاس (مثلاً: درس، آزمایشگاه، کنفرانس و ...)</summary>
        [StringLength(50)]
        public string? NoeClass { get; set; }

        // ============================================================
        // مشخصات فنی
        // ============================================================
        /// <summary>وضعیت کلاس (فعال/غیرفعال)</summary>
        public bool Vazeeyat { get; set; } = true;

        /// <summary>ظرفیت کلاس (تعداد دانشجو)</summary>
        public int? Zarfiat { get; set; }

        /// <summary>شماره طبقه</summary>
        public int? Tabagheh { get; set; }

        /// <summary>ظرفیت امتحانی</summary>
        public int? ZarfiatEmtahani { get; set; }

        // ============================================================
        // امکانات
        // ============================================================
        /// <summary>تعداد وایت‌برد</summary>
        public int? TedadWhiteboard { get; set; }

        /// <summary>آیا ویدئو پروژکتور دارد؟</summary>
        public bool Projector { get; set; } = false;

        /// <summary>سایر امکانات</summary>
        [StringLength(500)]
        public string? Emkanat { get; set; }

        /// <summary>توضیحات</summary>
        [StringLength(500)]
        public string? Tozihat { get; set; }

        // ============================================================
        // اطلاعات تماس
        // ============================================================
        /// <summary>شماره تلفن مسئول</summary>
        [StringLength(20)]
        public string? Telephon { get; set; }

        // ============================================================
        // فیلدهای سیستمی (اختیاری - پیشنهاد من)
        // ============================================================
        /// <summary>تاریخ ایجاد</summary>
        public DateTime? CreatedAt { get; set; }

        /// <summary>تاریخ آخرین ویرایش</summary>
        public DateTime? UpdatedAt { get; set; }

        // ============================================================
        // Navigation Properties (روابط با سایر جداول)
        // ============================================================
        // 🔥 جلسات این کلاس (فعلاً اینجا نمی‌ذاریم چون جدول Jalaseh هنوز ساخته نشده)
        // public virtual ICollection<Jalaseh>? Jalasehs { get; set; }
    }
}