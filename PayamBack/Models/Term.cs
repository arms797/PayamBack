using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    public class Term
    {
        [Key]
        [Required, MaxLength(50)]
        public string CodeTerm { get; set; } = "";

        [Required, MaxLength(100)]
        public string OnvanTerm { get; set; } = "";           // مثلاً "نیمسال اول ۱۴۰۴"

     
        [Column(TypeName = "date")]
        public DateOnly TermJari { get; set; }                 // تاریخ شروع ترم جاری

        [Column(TypeName = "date")]
        public DateOnly TarikheDastrasi { get; set; }          // تاریخ دسترسی به ترم جاری

        [Column(TypeName = "date")]
        public DateOnly TarikheEraeeDars { get; set; }         // تاریخ شروع ارائه (انتخاب واحد)

        [Column(TypeName = "date")]
        public DateOnly TarikhePayanDars { get; set; }         // تاریخ پایان ارائه (انتخاب واحد)

        [Column(TypeName = "date")]
        public DateOnly TarikheShorooClass { get; set; }       // تاریخ شروع کلاس‌ها

        [Column(TypeName = "date")]
        public DateOnly TarikhePayanClass { get; set; }        // تاریخ پایان کلاس‌ها

        [Column(TypeName = "date")]
        public DateOnly TarikheShorooMojavezMarakez { get; set; }  // شروع مجوز تدریس در سایر مراکز

        [Column(TypeName = "date")]
        public DateOnly TarikhePayanMojavezMarakez { get; set; }   // پایان مجوز تدریس در سایر مراکز

        public bool VazeeyatTerm { get; set; } = false;         // وضعیت ترم (فعال/غیرفعال)
    }
}
