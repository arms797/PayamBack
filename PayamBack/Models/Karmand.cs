using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// کارمند
    /// </summary>
    public class Karmand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // کد ملی
        [Required, MaxLength(10)]
        public string CodeMelli { get; set; } = "";

        // نام
        [Required, MaxLength(100)]
        public string Naam { get; set; } = "";

        // نام خانوادگی
        [Required, MaxLength(100)]
        public string NaameKhanevadeghi { get; set; } = "";

        // کلید خارجی به جدول مرکز خدمتی (Markaz)
        [Required]
        public int MarkazId { get; set; }

        // کلید خارجی به جدول مرکز اصلی (Markaz)
        [Required]
        public int MarkazAsliId { get; set; }

        // تلفن همراه ۱
        [Required, MaxLength(20)]
        public string Mobile { get; set; } = "";

        // تلفن همراه ۲
        [Required, MaxLength(20)]
        public string Mobile2 { get; set; } = "";

        // تلفن مستقیم محل کار
        [Required, MaxLength(20)]
        public string TelefonMostaghim { get; set; } = "";

        // تلفن غیر مستقیم محل کار
        [Required, MaxLength(20)]
        public string TelefonGhayreMostaghim { get; set; } = "";

        // شماره داخلی
        [Required, MaxLength(10)]
        public string TelefonDakheli { get; set; } = "";

        // ایمیل
        [Required, MaxLength(200)]
        public string Email { get; set; } = "";

        // امضا
        [MaxLength(250)]
        public string Emza { get; set; } = "";

        // ======== Navigation Properties ========

        // مرکز خدمتی
        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }

        // مرکز اصلی
        [ForeignKey(nameof(MarkazAsliId))]
        public Markaz? MarkazAsli { get; set; }
    }
}