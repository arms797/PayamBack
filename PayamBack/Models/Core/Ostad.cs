using PayamBack.Models.Schedule;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Core
{
    /// <summary>
    /// نوع همکاری استاد
    /// </summary>
    public enum NoeHamkariEnum
    {
        [Display(Name = "هیات علمی پیام نور")]
        HeyatElmiPayamNoor = 1,

        [Display(Name = "هیات علمی غیر پیام نور")]
        HeyatElmiGheyrePayamNoor = 2,

        [Display(Name = "مدرس مدعو")]
        ModaresMadov = 3,

        [Display(Name = "هیات علمی پیام نور (سایر استان ها)")]
        HeyatElmiPayamNoorSayerOstanha = 4
    }

    /// <summary>
    /// اطلاعات استادان
    /// </summary>
    public class Ostad
    {
        [Key]
        public int Id { get; set; }

        // کد دانشکده
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; } = "";

        // کد گروه آموزشی
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; } = "";

        // عنوان رشته تحصیلی استاد
        [MaxLength(100)]
        public string Reshteh { get; set; } = "";

        // کلید خارجی به جدول مرکز خدمتی
        [Required]
        public int MarkazId { get; set; }

        // کلید خارجی به جدول مرکز اصلی (اختیاری)
        public int? MarkazAsliId { get; set; }

        // کد استادی (شماره پرسنلی)
        [MaxLength(50)]
        public string CodeOstadi { get; set; } = "";

        // نام خانوادگی استاد
        [MaxLength(100)]
        public string NaamKhanevadegi { get; set; } = "";

        // نام استاد
        [MaxLength(100)]
        public string Naam { get; set; } = "";

        // جنس (مرد/زن)
        [MaxLength(10)]
        public string Jens { get; set; } = "";

        // نام پدر
        [MaxLength(100)]
        public string? NaamPedar { get; set; }

        // تاریخ تولد
        public DateTime? TarikhTavalod { get; set; }

        // شماره شناسنامه
        [MaxLength(20)]
        public string? ShomareShenasname { get; set; }

        // شماره ملی
        [MaxLength(10)]
        public string ShomareMelli { get; set; } = "";

        // ایمیل
        [MaxLength(200)]
        public string Email { get; set; } = "";

        // تلفن همراه ۱
        [MaxLength(15)]
        public string? Mobile { get; set; }

        // تلفن همراه ۲
        [MaxLength(15)]
        public string? Mobile2 { get; set; }

        // مرتبه علمی
        [MaxLength(50)]
        public string? MartabeElmi { get; set; }

        // سازمان مربوطه
        [MaxLength(100)]
        public string? SazmanMarboote { get; set; }

        // محل اشتغال
        [MaxLength(100)]
        public string? MahalEshteghal { get; set; }

        // امضا
        [MaxLength(250)]
        public string Emza { get; set; } = "";

        // وضعیت استاد (فعال یا غیر فعال)
        public bool Vazeeat { get; set; } = true;

        // نوع همکاری (Enum)
        [Required]
        public NoeHamkariEnum NoeHamkari { get; set; } = NoeHamkariEnum.ModaresMadov;

        // Navigation Properties
        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }

        [ForeignKey(nameof(MarkazAsliId))]
        public Markaz? MarkazAsli { get; set; }

        // Navigation Properties (ICollection)
        public ICollection<BarnamehHaftegiOstad>? BarnamehHaftegiOstads { get; set; }
        public ICollection<BarnamehTermiOstad>? BarnamehTermiOstads { get; set; }
    }
}