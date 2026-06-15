using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// اطلاعات دانشجویان
    /// </summary>
    public class Daneshjoo
    {
        // ======== شناسه فنی ========
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ======== کدهای شناسایی ========

        // کلید خارجی به جدول مرکز اصلی (Markaz)
        [Required]
        public int MarkazId { get; set; }

        // کلید خارجی به جدول مرکز آزمون (Markaz)
        [Required]
        public int MarkazAzmoonId { get; set; }

        // کلید خارجی به جدول مرکز ترمی (Markaz)
        [Required]
        public int MarkazTermiId { get; set; }

        // کلید خارجی به جدول رشته (Reshteh)
        [Required]
        public int ReshtehId { get; set; }

        // شماره دانشجویی
        [Required, MaxLength(50)]
        public string ShomareDaneshjooee { get; set; } = "";

        // ======== اطلاعات فردی ========

        // نام خانوادگی
        [Required, MaxLength(100)]
        public string NaamKhanevadegi { get; set; } = "";

        // نام
        [Required, MaxLength(100)]
        public string Naam { get; set; } = "";

        // جنس (مرد/زن)
        [MaxLength(10)]
        public string Jens { get; set; } = "";

        // نام پدر
        [MaxLength(100)]
        public string Naampedar { get; set; } = "";

        // شماره ملی
        [Required, MaxLength(10)]
        public string ShomareMelli { get; set; } = "";

        // شماره شناسنامه
        [MaxLength(20)]
        public string ShomareShenasname { get; set; } = "";

        // شماره گذرنامه / کارت هویت
        [MaxLength(50)]
        public string ShomareGozarnameYaKartHoviyat { get; set; } = "";

        // شناسه فراگیر اتباع خارجی
        [MaxLength(50)]
        public string ShenasayeFaragirAtbaaKhareji { get; set; } = "";

        // محل صدور
        [MaxLength(200)]
        public string MahalSodoor { get; set; } = "";

        // تاریخ تولد
        public DateOnly? TarikhTavalod { get; set; }

        // ======== اطلاعات تحصیلی ========

        // ترم ورود (مثلاً 1401-1)
        [Required, MaxLength(50)]
        public string TermVorood { get; set; } = "";

        // ======== وضعیت‌ها ========

        // شماره پرونده
        [MaxLength(50)]
        public string ShomareParvande { get; set; } = "";

        // وضعیت پرونده
        [MaxLength(100)]
        public string VazeeyatParvande { get; set; } = "";

        // ======== اطلاعات تماس ========

        // شماره تلفن همراه
        [MaxLength(15)]
        public string Mobile { get; set; } = "";

        // پست الکترونیک
        [MaxLength(200)]
        public string Email { get; set; } = "";

        // ======== سایر اطلاعات ========

        // چپ دست (بله/خیر)
        [MaxLength(10)]
        public string ChapDast { get; set; } = "";

        // شماره داوطلبی
        [MaxLength(50)]
        public string ShomareDavtalabi { get; set; } = "";

        // کد رشته محل قبولی سنجش
        [MaxLength(50)]
        public string CodeReshteMahalGhabooliSanjesh { get; set; } = "";

        // شماره سنجش
        [MaxLength(50)]
        public string ShomareSanjesh { get; set; } = "";

        // ======== Navigation Properties ========

        // مرکز اصلی
        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }

        // مرکز آزمون
        [ForeignKey(nameof(MarkazAzmoonId))]
        public Markaz? MarkazAzmoon { get; set; }

        // مرکز ترمی
        [ForeignKey(nameof(MarkazTermiId))]
        public Markaz? MarkazTermi { get; set; }

        // رشته تحصیلی
        [ForeignKey(nameof(ReshtehId))]
        public Reshteh? Reshteh { get; set; }
    }
}