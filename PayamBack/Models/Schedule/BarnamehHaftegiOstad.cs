using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    public class BarnamehHaftegiOstad
    {
        [Key]
        public int Id { get; set; }

        // اضافه کردن کلید خارجی جدول استاد  
        [Required]
        public int OstadId { get; set; }

        // کد استاد
        [Required, MaxLength(50)]
        public string CodeOstad { get; set; } = "";

        // کلید خارجی به جدول مرکز (Markaz)
        [Required]
        public int MarkazId { get; set; }

        // کد ترم
        [Required, MaxLength(50)]
        public string CodeTerm { get; set; } = "";

        // روز هفته (شنبه، یکشنبه، ...)
        [Required, MaxLength(50)]
        public string RoozeHafteh { get; set; } = "";

        // ساعات با کد وضعیت (ارجاع به VaziateSaatRules.Code)
        [Required]
        public int A { get; set; } = 0;  // 0 یعنی خالی/بدون وضعیت

        [Required]
        public int B { get; set; } = 0;

        [Required]
        public int C { get; set; } = 0;

        [Required]
        public int D { get; set; } = 0;

        [Required]
        public int E { get; set; } = 0;

        [Required]
        public int F { get; set; } = 0;

        [Required]
        public int G { get; set; } = 0;

        [Required]
        public int H { get; set; } = 0;

        // جزئیات بیشتر
        public bool Jozeiat { get; set; } = false;

        // توضیحات
        [MaxLength(500)]
        public string? Tozihat { get; set; }

        // Navigation properties
        [ForeignKey(nameof(OstadId))]
        public Ostad? Ostad { get; set; }

        [ForeignKey(nameof(CodeTerm))]
        public Term? Term { get; set; }

        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }
    }
}