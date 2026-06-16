using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    // تقویم ترم (تعطیلات، مناسبت‌ها، هفته‌ها)
    public class TaghvimTermi
    {
        [Key]
        public int Id { get; set; }

        // کلید خارجی به جدول ترم (CodeTerm کلید اصلی جدول Term است)
        [Required, MaxLength(50)]
        public string CodeTerm { get; set; } = "";

        // تاریخ
        [Required]
        public DateOnly Tarikh { get; set; }

        // کد روز (مثلاً 1=شنبه، 2=یکشنبه، ...)
        [Required]
        public int CodeRooz { get; set; }

        // نام روز هفته
        [Required, MaxLength(20)]
        public string RoozHafteh { get; set; } = "";

        // کد هفته (هفته چندم ترم)
        [Required]
        public int CodeHafteh { get; set; }

        // نام هفته (هفته اول، هفته دوم، ...)
        [Required, MaxLength(50)]
        public string Hafteh { get; set; } = "";

        // کد ساعت تعطیلی (اختیاری)
        [MaxLength(10)]
        public string? CodeSaateTatili { get; set; }

        // عنوان مناسبت (اختیاری)
        [MaxLength(200)]
        public string? OnvanMonasebat { get; set; }

        // توضیحات (اختیاری)
        [MaxLength(200)]
        public string? Tozihat { get; set; }

        // وضعیت روز (فعال/غیرفعال)
        [Required]
        public bool Vazeeyat { get; set; } = true;
    }
}