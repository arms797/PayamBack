using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class Term
    {
        /// <summary>کلید اصلی (کد ترم)</summary>
        [Key]
        [MaxLength(50)]
        public string? CodeTerm { get; set; }

        /// <summary>عنوان ترم (مثلاً "نیمسال اول ۱۴۰۴")</summary>
        [MaxLength(100)]
        public string? OnvanTerm { get; set; }

        /// <summary>تاریخ شروع ترم جاری</summary>
        [Column(TypeName = "date")]
        public DateOnly? TermJari { get; set; }

        /// <summary>تاریخ دسترسی به ترم جاری</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikheDastrasi { get; set; }

        /// <summary>تاریخ شروع ارائه (انتخاب واحد)</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikheEraeeDars { get; set; }

        /// <summary>تاریخ پایان ارائه (انتخاب واحد)</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikhePayanDars { get; set; }

        /// <summary>تاریخ شروع کلاس‌ها</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikheShorooClass { get; set; }

        /// <summary>تاریخ پایان کلاس‌ها</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikhePayanClass { get; set; }

        /// <summary>شروع مجوز تدریس در سایر مراکز</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikheShorooMojavezMarakez { get; set; }

        /// <summary>پایان مجوز تدریس در سایر مراکز</summary>
        [Column(TypeName = "date")]
        public DateOnly? TarikhePayanMojavezMarakez { get; set; }

        /// <summary>وضعیت ترم (فعال/غیرفعال)</summary>
        public bool? Vazeeyat { get; set; }
    }
}