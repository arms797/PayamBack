
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// اطلاعات استادان
    /// </summary>
    public class Ostad
    {
        // ======== شناسه فنی (توصیه‌شده، در صورت نیاز می‌توانید حذف کنید) ========
        [Key]
        public int Id { get; set; }

        // ======== کدهای شناسایی ========
        /// <summary>کد دانشکده</summary>
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; }

        /// <summary>کد گروه آموزشی</summary>
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; }

        /// <summary>کد رشته (دو رقمی)</summary>
        [MaxLength(2)]
        public string CodeReshteDoRaghami { get; set; }

        /// <summary>کد استان استاد</summary>
        [MaxLength(50)]
        public string CodeOstan { get; set; }

        /// <summary>کد واحد / مرکز</summary>
        [MaxLength(50)]
        public string CodeMarkaz { get; set; }

        /// <summary>کد استادی (شماره پرسنلی)</summary>
        [MaxLength(50)]
        public string CodeOstadi { get; set; }

        // ======== اطلاعات فردی ========
        /// <summary>نام خانوادگی استاد</summary>
        [MaxLength(100)]
        public string NaamKhanevadegiOstad { get; set; }

        /// <summary>نام استاد</summary>
        [MaxLength(100)]
        public string NaamOstad { get; set; }

        /// <summary>جنس (مرد/زن)</summary>
        [MaxLength(10)]
        public string Jens { get; set; }

        /// <summary>نام پدر</summary>
        [MaxLength(100)]
        public string NaamPedar { get; set; }

        /// <summary>تاریخ تولد</summary>
        public DateTime? TarikhTavalod { get; set; }

        /// <summary>ملیت</summary>
        [MaxLength(50)]
        public string Melliyat { get; set; }

        /// <summary>محل تولد</summary>
        [MaxLength(200)]
        public string MahalTavalod { get; set; }

        /// <summary>شماره شناسنامه</summary>
        [MaxLength(20)]
        public string ShomareShenasname { get; set; }

        /// <summary>محل صدور شناسنامه</summary>
        [MaxLength(200)]
        public string MahalSoddorShenasname { get; set; }

        /// <summary>شماره ملی</summary>
        [MaxLength(10)]
        public string ShomareMelli { get; set; }

        // ======== اطلاعات تماس ========
        /// <summary>آدرس پست الکترونیکی</summary>
        [MaxLength(200)]
        public string AdresPostElektroniki { get; set; }

        /// <summary>آدرس محل سکونت</summary>
        [MaxLength(500)]
        public string Adres { get; set; }

        /// <summary>کد پستی استاد</summary>
        [MaxLength(20)]
        public string CodePostiOstad { get; set; }

        /// <summary>تلفن همراه ۱</summary>
        [MaxLength(15)]
        public string TelHamrah1 { get; set; }

        /// <summary>تلفن همراه ۲</summary>
        [MaxLength(15)]
        public string TelHamrah2 { get; set; }

        // ======== اطلاعات شغلی ========
        /// <summary>تاریخ پایان کار</summary>
        public DateTime? TarikhPayanKar { get; set; }

        /// <summary>پایه</summary>
        [MaxLength(50)]
        public string Payeh { get; set; }

        /// <summary>مرتبه علمی</summary>
        [MaxLength(50)]
        public string MartabeElmi { get; set; }

        /// <summary>حالت استاد</summary>
        [MaxLength(50)]
        public string HaalatOstad { get; set; }

        /// <summary>تاریخ استخدام</summary>
        public DateTime? TarikhEstekhdam { get; set; }

        /// <summary>وضعیت استخدام</summary>
        [MaxLength(50)]
        public string VazeeyatEstekhdam { get; set; }

        /// <summary>نوع و شماره بیمه</summary>
        [MaxLength(50)]
        public string NooVaShomareBimeh { get; set; }

        // ======== وضعیت‌ها ========
        /// <summary>آخرین وضعیت</summary>
        [MaxLength(100)]
        public string AaKharinVazeeyat { get; set; }

        /// <summary>تاریخ آخرین وضعیت</summary>
        public DateTime? TarikhAakharinVazeeyat { get; set; }

        /// <summary>ارائه درس (بله/خیر)</summary>
        [MaxLength(10)]
        public string EraehDars { get; set; }

        // ======== سازمان و محل اشتغال ========
        /// <summary>سازمان مربوطه</summary>
        [MaxLength(200)]
        public string SazmanMarboote { get; set; }

        /// <summary>محل اشتغال</summary>
        [MaxLength(200)]
        public string MahalEshteghal { get; set; }

        /// <summary>امضا</summary>
        [MaxLength(250)]
        public string Emza { get; set; } = "";

        /// <summary>کد نوع همکاری</summary>هیلات علمی پیام نور=1،هیات علمی سایر دانشگاههای دولتی=2،مدرس مدعو=3
        [Required]
        public int CodeNoeHamkari { get; set; } = 3;
        /// <summary>نوع همکاری</summary>هیلات علمی پیام نور=1،هیات علمی سایر دانشگاههای دولتی=2،مدرس مدعو=3
        [Required]
        [MaxLength(50)]
        public string NoeHamkari { get; set; } = "مدرس مدعو";

    }
}
