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
        // ======== شناسه فنی (توصیه‌شده) ========
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ======== اطلاعات اصلی امکان ========
        /// <summary>کد امکان (یکتا)</summary>
        [Required]
        public int Code { get; set; }// با Unique Index

        /// <summary>نام امکان</summary>
        [Required]
        [MaxLength(200)]
        public string NaamEmkanat { get; set; }

        /// <summary>سرتیتر / گروه امکان</summary>
        [Required]
        [MaxLength(200)]
        public string SarTitrEmkanat { get; set; }

        /// <summary>مسیر کامپوننت (ویژه فرانت‌اند: Angular/React/Blazor)</summary>
        [Required]
        [MaxLength(300)]
        public string Component { get; set; }

        // ======== ترتیب نمایش ========
        /// <summary>ترتیب نمایش سرتیتر در منو</summary>
        [Required]
        public int TartibNamayeshSarTitr { get; set; }

        /// <summary>ترتیب نمایش این امکان در پنل/منو</summary>
        [Required]
        public int TartibNamayeshEmkan { get; set; }

        // ======== وضعیت ========
        /// <summary>وضعیت (true = فعال)</summary>
        [Required]
        public bool Vazeeyat { get; set; }
    }
}
