using PayamBack.Models.Core;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// رشته‌های تحصیلی
    /// </summary>
    public class Reshteh
    {
        // ======== شناسه فنی ========
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ======== کدهای شناسایی ========

        /// <summary>کد مقطع</summary>
        [MaxLength(50)]
        public string? CodeMaghta { get; set; }

        /// <summary>مقطع تحصیلی (کارشناسی، ارشد، دکتری)</summary>
        [MaxLength(100)]
        public string? Maghta { get; set; }

        /// <summary>کلید خارجی به جدول گروه آموزشی</summary>
        public int? GrooheAmoozeshiId { get; set; }

        /// <summary>کد دو رقمی رشته</summary>
        [MaxLength(2)]
        public string? CodeReshteDoRaghami { get; set; }

        /// <summary>کد رشته</summary>
        [MaxLength(50)]
        public string? CodeReshte { get; set; }

        // ======== عنوان رشته ========

        /// <summary>عنوان رشته</summary>
        [MaxLength(200)]
        public string? OnvanReshte { get; set; }

        // ======== اطلاعات ترم ========

        /// <summary>ترم ورود</summary>
        [MaxLength(10)]
        public string? TermVorood { get; set; }

        /// <summary>ترم اعمال</summary>
        [MaxLength(10)]
        public string? TermEamal { get; set; }

        // ======== Navigation Properties ========

        /// <summary>گروه آموزشی</summary>
        [ForeignKey(nameof(GrooheAmoozeshiId))]
        public virtual GrooheAmoozeshi? GrooheAmoozeshi { get; set; }

        /// <summary>دانشجویان مرتبط با این رشته</summary>
        public virtual ICollection<Daneshjoo>? Daneshjoos { get; set; }
    }
}