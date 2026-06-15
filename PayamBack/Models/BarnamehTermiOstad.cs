using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    public class BarnamehTermiOstad
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

        // روز هفته (برای سرعت جستجو)
        [Required, MaxLength(50)]
        public string RoozeHafteh { get; set; } = "";

        // تاریخ خاص در ترم
        [Required, Column(TypeName = "date")]
        public DateOnly Tarikh { get; set; }

        // ساعات (کد وضعیت از VaziateSaatRules)
        [Required]
        public int A { get; set; } = 0;
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

        // وضعیت پر شدن هر ساعت
        [Required]
        public bool TA { get; set; } = false;
        [Required]
        public bool TB { get; set; } = false;
        [Required]
        public bool TC { get; set; } = false;
        [Required]
        public bool TD { get; set; } = false;
        [Required]
        public bool TE { get; set; } = false;
        [Required]
        public bool TF { get; set; } = false;
        [Required]
        public bool TG { get; set; } = false;
        [Required]
        public bool TH { get; set; } = false;

        // فعال/غیرفعال
        [Required]
        public bool Faal { get; set; } = true;

        // Navigation properties
        [ForeignKey(nameof(OstadId))]
        public Ostad? Ostad { get; set; }

        [ForeignKey(nameof(CodeTerm))]
        public Term? Term { get; set; }

        [ForeignKey(nameof(MarkazId))]
        public Markaz? Markaz { get; set; }
    }
}