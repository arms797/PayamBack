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
        // ======== شناسه فنی (توصیه‌شده) ========
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ======== کدهای شناسایی ========
        /// <summary>کد مرکز</summary>
        [MaxLength(50)]
        public string CodeMarkaz { get; set; }

        /// <summary>کد مرکز آزمون</summary>
        [MaxLength(50)]
        public string CodeMarkazeAzmoon { get; set; }

        /// <summary>کد مرکز ترمی</summary>
        [MaxLength(50)]
        public string CodeMarkazeTermi { get; set; }

        /// <summary>کد استان واحد</summary>
        [MaxLength(50)]
        public string CodeOstan { get; set; }

        /// <summary>شماره دانشجویی</summary>
        [Required]
        [MaxLength(50)]
        public string ShomareDaneshjooee { get; set; }

        // ======== اطلاعات فردی ========
        /// <summary>نام خانوادگی</summary>
        [Required]
        [MaxLength(100)]
        public string NaamKhanevadegi { get; set; }

        /// <summary>نام</summary>
        [Required]
        [MaxLength(100)]
        public string Naam { get; set; }

        /// <summary>جنس (مرد/زن)</summary>
        [MaxLength(10)]
        public string Jens { get; set; }

        /// <summary>نام پدر</summary>
        [MaxLength(100)]
        public string Naampedar { get; set; }

        /// <summary>شماره ملی</summary>
        [Required]
        [MaxLength(10)]
        public string ShomareMelli { get; set; }

        /// <summary>شماره شناسنامه</summary>
        [MaxLength(20)]
        public string ShomareShenasname { get; set; }

        /// <summary>شماره گذرنامه / کارت هویت</summary>
        [MaxLength(50)]
        public string ShomareGozarnameYaKartHoviyat { get; set; }

        /// <summary>شناسه فراگیر اتباع خارجی</summary>
        [MaxLength(50)]
        public string ShenasayeFaragirAtbaaKhareji { get; set; }

        /// <summary>محل صدور</summary>
        [MaxLength(200)]
        public string MahalSodoor { get; set; }

        /// <summary>تاریخ تولد</summary>
        [Required]
        public DateTime? TarikhTavalod { get; set; }

        // ======== اطلاعات تحصیلی ========
        /// <summary>ترم ورود (مثلاً 1401-1)</summary>
        [Required]
        [MaxLength(10)]
        public string TermVorood { get; set; }   // در مستند اولیه date ذکر شده بود اما معمولاً رشته است

        /// <summary>کد دانشکده</summary>
        [Required]
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; }

        /// <summary>کد گروه آموزشی</summary>
        [Required]
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; }

        /// <summary>کد رشته دو رقمی</summary>
        [Required]
        [MaxLength(2)]
        public string CodeReshteDoRaghami { get; set; }

        /// <summary>کد مقطع</summary>
        [MaxLength(50)]
        public string CodeMaghta { get; set; }

        /// <summary>دوره</summary>
        [MaxLength(50)]
        public string Dore { get; set; }

        /// <summary>معدل کل</summary>
        public float? MoadelKol { get; set; }

        /// <summary>واحد گذرانده</summary>
        public float? VahedGozarande { get; set; }

        /// <summary>واحد اخذ شده</summary>
        public float? VahedAkhzShode { get; set; }

        // ======== وضعیت‌ها ========
        /// <summary>آخرین وضعیت دانشجو</summary>
        [MaxLength(100)]
        public string AkharinVazeeyatDaneshjoo { get; set; }

        /// <summary>وضعیت نظام وظیفه</summary>
        [MaxLength(100)]
        public string VazeeyatNezamVazife { get; set; }

        /// <summary>شماره پرونده</summary>
        [MaxLength(50)]
        public string ShomareParvande { get; set; }

        /// <summary>وضعیت پرونده</summary>
        [MaxLength(100)]
        public string VazeeyatParvande { get; set; }

        // ======== اطلاعات تماس ========
        /// <summary>شماره تلفن</summary>
        [MaxLength(15)]
        public string ShomareTelefon { get; set; }

        /// <summary>تلفن همراه</summary>
        [MaxLength(15)]
        public string TelefonHamrah { get; set; }

        /// <summary>کد پستی</summary>
        [MaxLength(20)]
        public string CodePosti { get; set; }

        /// <summary>نام استان محل سکونت</summary>
        [MaxLength(100)]
        public string NaamOstanMahalSokoonat { get; set; }

        /// <summary>شهر محل سکونت</summary>
        [MaxLength(100)]
        public string ShahrmahalSokoonat { get; set; }

        /// <summary>آدرس</summary>
        [MaxLength(500)]
        public string Adres { get; set; }

        /// <summary>پست الکترونیک</summary>
        [MaxLength(200)]
        public string POstElektronik { get; set; }

        // ======== سایر اطلاعات ========
        /// <summary>چپ دست (بله/خیر)</summary>
        [MaxLength(10)]
        public string ChapDast { get; set; }

        /// <summary>شماره داوطلبی</summary>
        [MaxLength(50)]
        public string ShomareDavtalabi { get; set; }

        /// <summary>کد رشته محل قبولی سنجش</summary>
        [MaxLength(50)]
        public string CodeReshteMahalGhabooliSanjesh { get; set; }

        /// <summary>شماره سنجش</summary>
        [MaxLength(50)]
        public string ShomareSanjesh { get; set; }
    }
}
