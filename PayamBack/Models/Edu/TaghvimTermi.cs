using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// تقویم ترم (تعطیلات، مناسبت‌ها، هفته‌ها)
    /// </summary>
    public class TaghvimTermi
    {
        [Key]
        public int Id { get; set; }

        /// <summary>کلید خارجی به جدول ترم (CodeTerm کلید اصلی جدول Term است)</summary>
        [MaxLength(50)]
        public string? CodeTerm { get; set; }

        /// <summary>تاریخ</summary>
        public DateOnly? Tarikh { get; set; }

        /// <summary>کد روز (مثلاً 1=شنبه، 2=یکشنبه، ...)</summary>
        public int? CodeRooz { get; set; }

        /// <summary>نام روز هفته</summary>
        [MaxLength(20)]
        public string? RoozHafteh { get; set; }

        /// <summary>کد هفته (هفته چندم ترم)</summary>
        public int? CodeHafteh { get; set; }

        /// <summary>نام هفته (هفته اول، هفته دوم، ...)</summary>
        [MaxLength(50)]
        public string? Hafteh { get; set; }

        /// <summary>کد ساعت تعطیلی (اختیاری)</summary>
        [MaxLength(10)]
        public string? CodeSaateTatili { get; set; }

        /// <summary>عنوان مناسبت (اختیاری)</summary>
        [MaxLength(200)]
        public string? OnvanMonasebat { get; set; }

        /// <summary>توضیحات (اختیاری)</summary>
        [MaxLength(200)]
        public string? Tozihat { get; set; }

        /// <summary>وضعیت روز (فعال/غیرفعال)</summary>
        public bool? Vazeeyat { get; set; }
    }
}