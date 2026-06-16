using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    public class BarnamehTermiOstad
    {
        [Key]
        public int Id { get; set; }

        /// <summary>کلید خارجی به جدول استاد</summary>
        public int? OstadId { get; set; }

        /// <summary>کد استاد</summary>
        [MaxLength(50)]
        public string? CodeOstad { get; set; }

        /// <summary>کلید خارجی به جدول مرکز (Markaz)</summary>
        public int? MarkazId { get; set; }

        /// <summary>کد ترم</summary>
        [MaxLength(50)]
        public string? CodeTerm { get; set; }

        /// <summary>روز هفته (برای سرعت جستجو)</summary>
        [MaxLength(50)]
        public string? RoozeHafteh { get; set; }

        /// <summary>تاریخ خاص در ترم</summary>
        [Column(TypeName = "date")]
        public DateOnly? Tarikh { get; set; }

        /// <summary>ساعت A (کد وضعیت از VaziateSaatRules)</summary>
        public int? A { get; set; }

        /// <summary>ساعت B</summary>
        public int? B { get; set; }

        /// <summary>ساعت C</summary>
        public int? C { get; set; }

        /// <summary>ساعت D</summary>
        public int? D { get; set; }

        /// <summary>ساعت E</summary>
        public int? E { get; set; }

        /// <summary>ساعت F</summary>
        public int? F { get; set; }

        /// <summary>ساعت G</summary>
        public int? G { get; set; }

        /// <summary>ساعت H</summary>
        public int? H { get; set; }

        /// <summary>وضعیت پر شدن ساعت A</summary>
        public bool? TA { get; set; }

        /// <summary>وضعیت پر شدن ساعت B</summary>
        public bool? TB { get; set; }

        /// <summary>وضعیت پر شدن ساعت C</summary>
        public bool? TC { get; set; }

        /// <summary>وضعیت پر شدن ساعت D</summary>
        public bool? TD { get; set; }

        /// <summary>وضعیت پر شدن ساعت E</summary>
        public bool? TE { get; set; }

        /// <summary>وضعیت پر شدن ساعت F</summary>
        public bool? TF { get; set; }

        /// <summary>وضعیت پر شدن ساعت G</summary>
        public bool? TG { get; set; }

        /// <summary>وضعیت پر شدن ساعت H</summary>
        public bool? TH { get; set; }

        /// <summary>فعال/غیرفعال</summary>
        public bool? Faal { get; set; }

        // ======== Navigation Properties ========

        /// <summary>استاد مرتبط</summary>
        [ForeignKey(nameof(OstadId))]
        public virtual Ostad? Ostad { get; set; }

        /// <summary>ترم مرتبط</summary>
        [ForeignKey(nameof(CodeTerm))]
        public virtual Term? Term { get; set; }

        /// <summary>مرکز مرتبط</summary>
        [ForeignKey(nameof(MarkazId))]
        public virtual Markaz? Markaz { get; set; }
    }
}