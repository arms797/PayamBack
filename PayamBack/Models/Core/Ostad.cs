using PayamBack.Models.Edu;
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

        /// <summary>کلید خارجی به جدول مرکز خدمتی</summary>
        public int? MarkazId { get; set; }

        /// <summary>کلید خارجی به جدول مرکز اصلی (اختیاری)</summary>
        public int? MarkazAsliId { get; set; }      

        /// <summary>کد استادی (شماره پرسنلی)</summary>
        [MaxLength(50)]
        public string? CodeOstadi { get; set; }

        /// <summary>نام خانوادگی استاد</summary>
        [MaxLength(100)]
        public string? NaamKhanevadegi { get; set; }

        /// <summary>نام استاد</summary>
        [MaxLength(100)]
        public string? Naam { get; set; }

        /// <summary>جنس (مرد/زن)</summary>
        [MaxLength(10)]
        public string? Jens { get; set; }

        /// <summary>نام پدر</summary>
        [MaxLength(100)]
        public string? NaamPedar { get; set; }

        /// <summary>تاریخ تولد</summary>
        [MaxLength(10)]
        public string TarikhTavalod { get; set; }

        /// <summary>شماره شناسنامه</summary>
        [MaxLength(20)]
        public string? ShomareShenasname { get; set; }

        /// <summary>شماره ملی</summary>
        [MaxLength(10)]
        public string? ShomareMelli { get; set; }

        /// <summary>ایمیل</summary>
        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>تلفن همراه ۱</summary>
        [MaxLength(15)]
        public string? Mobile { get; set; }

        /// <summary>تلفن همراه ۲</summary>
        [MaxLength(15)]
        public string? Mobile2 { get; set; }

        /// <summary>مرتبه علمی</summary>
        [MaxLength(50)]
        public string? MartabeElmi { get; set; }

        /// <summary>سازمان مربوطه</summary>
        [MaxLength(100)]
        public string? SazmanMarboote { get; set; }

        /// <summary>محل اشتغال</summary>
        [MaxLength(100)]
        public string? MahalEshteghal { get; set; }

        /// <summary>امضا</summary>
        [MaxLength(250)]
        public string? Emza { get; set; }

        /// <summary>وضعیت استاد (فعال یا غیر فعال)</summary>
        public bool? Vazeeat { get; set; }

        /// <summary>نوع همکاری (Enum)</summary>
        public NoeHamkariEnum? NoeHamkari { get; set; }

        /// <summary>نوع بیمه</summary>
        [MaxLength(250)]
        public string? NoeBimeh { get; set; }

        /// <summary>شماره بیمه</summary>
        [MaxLength(50)]
        public string? ShomarehBimeh { get; set; }        

        // ======== Navigation Properties ========

        /// <summary>مرکز خدمتی</summary>
        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }

        /// <summary>مرکز اصلی</summary>
        [ForeignKey(nameof(MarkazAsliId))]
        public virtual Markaz? MarkazAsli { get; set; }

        /// <summary>برنامه هفتگی اساتید</summary>
        public virtual ICollection<BarnamehHaftegiOstad>? BarnamehHaftegiOstads { get; set; }

        /// <summary>برنامه ترمی اساتید</summary>
        public virtual ICollection<BarnamehTermiOstad>? BarnamehTermiOstads { get; set; }

        /// <summary>مدارک تحصیلی اساتید</summary>
        public virtual ICollection<OstadMadrak>? OstadMadraks { get; set; }
    }
}