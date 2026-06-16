using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    /// <summary>
    /// امکانات و دسترسی‌های سیستم (منوها، صفحات، دکمه‌ها و ...)
    /// </summary>
    public class Emkanat
    {
        // ======== شناسه فنی ========
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ======== اطلاعات اصلی امکان ========

        /// <summary>کد امکان (یکتا) - با Unique Index</summary>
        public int? Code { get; set; }

        /// <summary>نام امکان</summary>
        [MaxLength(200)]
        public string? NaamEmkanat { get; set; }

        /// <summary>سرتیتر / گروه امکان</summary>
        [MaxLength(200)]
        public string? SarTitrEmkanat { get; set; }

        /// <summary>مسیر کامپوننت (ویژه فرانت‌اند: Angular/React/Blazor)</summary>
        [MaxLength(300)]
        public string? Component { get; set; }

        // ======== ترتیب نمایش ========

        /// <summary>ترتیب نمایش سرتیتر در منو</summary>
        public int? TartibNamayeshSarTitr { get; set; }

        /// <summary>ترتیب نمایش این امکان در پنل/منو</summary>
        public int? TartibNamayeshEmkan { get; set; }

        // ======== وضعیت ========

        /// <summary>وضعیت (true = فعال)</summary>
        public bool? Vazeeyat { get; set; }
    }
}