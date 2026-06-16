using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Core
{
    /// <summary>
    /// کارمند
    /// </summary>
    public class Karmand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>کد ملی</summary>
        [MaxLength(10)]
        public string? CodeMelli { get; set; }

        /// <summary>نام</summary>
        [MaxLength(100)]
        public string? Naam { get; set; }

        /// <summary>نام خانوادگی</summary>
        [MaxLength(100)]
        public string? NaameKhanevadeghi { get; set; }

        /// <summary>کلید خارجی به جدول مرکز خدمتی (Markaz)</summary>
        public int? MarkazId { get; set; }

        /// <summary>کلید خارجی به جدول مرکز اصلی (Markaz)</summary>
        public int? MarkazAsliId { get; set; }

        /// <summary>تلفن همراه ۱</summary>
        [MaxLength(20)]
        public string? Mobile { get; set; }

        /// <summary>تلفن همراه ۲</summary>
        [MaxLength(20)]
        public string? Mobile2 { get; set; }

        /// <summary>تلفن مستقیم محل کار</summary>
        [MaxLength(20)]
        public string? TelefonMostaghim { get; set; }

        /// <summary>تلفن غیر مستقیم محل کار</summary>
        [MaxLength(20)]
        public string? TelefonGhayreMostaghim { get; set; }

        /// <summary>شماره داخلی</summary>
        [MaxLength(10)]
        public string? TelefonDakheli { get; set; }

        /// <summary>ایمیل</summary>
        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>امضا</summary>
        [MaxLength(250)]
        public string? Emza { get; set; }

        // ======== Navigation Properties ========

        /// <summary>مرکز خدمتی</summary>
        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }

        /// <summary>مرکز اصلی</summary>
        [ForeignKey(nameof(MarkazAsliId))]
        public virtual Markaz? MarkazAsli { get; set; }
    }
}