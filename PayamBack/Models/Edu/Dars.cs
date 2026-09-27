using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// جدول درس‌ها
    /// کلیه درس‌های موجود در دانشگاه در این جدول ذخیره می‌شوند
    /// </summary>
    [Table("Dars")]
    public class Dars
    {
        // ============================================================
        // کلید اصلی
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // اطلاعات اصلی درس
        // ============================================================
        /// <summary>کد درس</summary>
        [Required(ErrorMessage = "کد درس الزامی است")]
        [StringLength(20)]
        public string CodeDars { get; set; } = string.Empty;

        /// <summary>نام درس</summary>
        [Required(ErrorMessage = "نام درس الزامی است")]
        [StringLength(200)]
        public string NaamDars { get; set; } = string.Empty;

        // ============================================================
        // واحدها
        // ============================================================
        /// <summary>واحد تئوری</summary>
        [Column(TypeName = "decimal(4,2)")]
        public decimal? VahedTeori { get; set; }

        /// <summary>واحد عملی</summary>
        [Column(TypeName = "decimal(4,2)")]
        public decimal? VahedAmali { get; set; }

        // ============================================================
        // ساعت‌ها
        // ============================================================
        /// <summary>ساعت تئوری اصلی (طبق مصوبه)</summary>
        public int? SaatTeoriOrginal { get; set; }

        /// <summary>ساعت عملی اصلی (طبق مصوبه)</summary>
        public int? SaatAmaliOrginal { get; set; }

        /// <summary>ساعت تئوری قابل اعمال</summary>
        public int? SaatTeori { get; set; }

        /// <summary>ساعت عملی قابل اعمال</summary>
        public int? SaatAmali { get; set; }

        // ============================================================
        // اطلاعات ترم و نوع
        // ============================================================
        /// <summary>ترم اخذ</summary>
        public int? TermAkhz { get; set; }

        /// <summary>نوع درس</summary>
        [StringLength(50)]
        public string? NoeDars { get; set; }

        /// <summary>نوع آزمون</summary>
        [StringLength(50)]
        public string? NoeAzmoon { get; set; }

        // ============================================================
        // روابط
        // ============================================================
        /// <summary>شناسه رشته (FK به Rashteh)</summary>
        public int? ReshtehId { get; set; }

        [ForeignKey(nameof(ReshtehId))]
        public virtual Reshteh? Reshteh { get; set; }

        // ============================================================
        // ظرفیت
        // ============================================================
        /// <summary>ظرفیت پیش‌فرض</summary>
        public int? Zarfiat { get; set; }

        // ============================================================
        // فیلدهای سیستمی
        // ============================================================
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }

        // ============================================================
        // Navigation Properties
        // ============================================================
        // 🔥 منابع درسی این درس
         public virtual ICollection<ManbaDars>? ManbaDarsList { get; set; }

        // 🔥 دروس ارائه شده این درس
        // public virtual ICollection<DoroushAraehShode>? DoroushAraehShodes { get; set; }
    }
}