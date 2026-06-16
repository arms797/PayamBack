using PayamBack.Models.Edu;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Core
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

        /// <summary>کلید خارجی به جدول مرکز اصلی (Markaz)</summary>
        public int? MarkazId { get; set; }

        /// <summary>کلید خارجی به جدول مرکز آزمون (Markaz)</summary>
        public int? MarkazAzmoonId { get; set; }

        /// <summary>کلید خارجی به جدول مرکز ترمی (Markaz)</summary>
        public int? MarkazTermiId { get; set; }

        /// <summary>کلید خارجی به جدول رشته (Reshteh)</summary>
        public int? ReshtehId { get; set; }

        /// <summary>شماره دانشجویی</summary>
        [MaxLength(50)]
        public string? ShomareDaneshjooee { get; set; }

        // ======== اطلاعات فردی ========

        /// <summary>نام خانوادگی</summary>
        [MaxLength(100)]
        public string? NaamKhanevadegi { get; set; }

        /// <summary>نام</summary>
        [MaxLength(100)]
        public string? Naam { get; set; }

        /// <summary>جنس (مرد/زن)</summary>
        [MaxLength(10)]
        public string? Jens { get; set; }

        /// <summary>نام پدر</summary>
        [MaxLength(100)]
        public string? Naampedar { get; set; }

        /// <summary>شماره ملی</summary>
        [MaxLength(10)]
        public string? ShomareMelli { get; set; }

        /// <summary>شماره شناسنامه</summary>
        [MaxLength(20)]
        public string? ShomareShenasname { get; set; }

        /// <summary>شماره گذرنامه / کارت هویت</summary>
        [MaxLength(50)]
        public string? ShomareGozarnameYaKartHoviyat { get; set; }

        /// <summary>شناسه فراگیر اتباع خارجی</summary>
        [MaxLength(50)]
        public string? ShenasayeFaragirAtbaaKhareji { get; set; }

        /// <summary>محل صدور</summary>
        [MaxLength(200)]
        public string? MahalSodoor { get; set; }

        /// <summary>تاریخ تولد</summary>
        public DateOnly? TarikhTavalod { get; set; }

        // ======== اطلاعات تحصیلی ========

        /// <summary>ترم ورود (مثلاً 1401-1)</summary>
        [MaxLength(50)]
        public string? TermVorood { get; set; }

        // ======== وضعیت‌ها ========

        /// <summary>شماره پرونده</summary>
        [MaxLength(50)]
        public string? ShomareParvande { get; set; }

        /// <summary>وضعیت پرونده</summary>
        [MaxLength(100)]
        public string? VazeeyatParvande { get; set; }

        // ======== اطلاعات تماس ========

        /// <summary>شماره تلفن همراه</summary>
        [MaxLength(15)]
        public string? Mobile { get; set; }

        /// <summary>پست الکترونیک</summary>
        [MaxLength(200)]
        public string? Email { get; set; }

        // ======== سایر اطلاعات ========

        /// <summary>چپ دست (بله/خیر)</summary>
        [MaxLength(10)]
        public string? ChapDast { get; set; }

        /// <summary>شماره داوطلبی</summary>
        [MaxLength(50)]
        public string? ShomareDavtalabi { get; set; }

        /// <summary>کد رشته محل قبولی سنجش</summary>
        [MaxLength(50)]
        public string? CodeReshteMahalGhabooliSanjesh { get; set; }

        /// <summary>شماره سنجش</summary>
        [MaxLength(50)]
        public string? ShomareSanjesh { get; set; }

        // ======== Navigation Properties ========

        /// <summary>مرکز اصلی</summary>
        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }

        /// <summary>مرکز آزمون</summary>
        [ForeignKey(nameof(MarkazAzmoonId))]
        public virtual Markaz? MarkazAzmoon { get; set; }

        /// <summary>مرکز ترمی</summary>
        [ForeignKey(nameof(MarkazTermiId))]
        public virtual Markaz? MarkazTermi { get; set; }

        /// <summary>رشته تحصیلی</summary>
        [ForeignKey(nameof(ReshtehId))]
        public virtual Reshteh? Reshteh { get; set; }
    }
}