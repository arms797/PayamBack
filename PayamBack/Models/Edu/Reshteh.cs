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

        // کد مقطع
        [Required, MaxLength(50)]
        public string CodeMaghta { get; set; } = "";

        // مقطع تحصیلی (کارشناسی، ارشد، دکتری)
        [Required, MaxLength(100)]
        public string Maghta { get; set; } = "";

        // کلید خارجی به جدول گروه آموزشی
        [Required]
        public int GrooheAmoozeshiId { get; set; }

        // کد دو رقمی رشته
        [Required, MaxLength(2)]
        public string CodeReshteDoRaghami { get; set; } = "";

        // کد رشته
        [Required, MaxLength(50)]
        public string CodeReshte { get; set; } = "";

        // ======== عنوان رشته ========

        // عنوان رشته
        [Required, MaxLength(200)]
        public string OnvanReshte { get; set; } = "";

        // ======== اطلاعات ترم ========

        // ترم ورود
        [Required, MaxLength(10)]
        public string TermVorood { get; set; } = "";

        // ترم اعمال
        [Required, MaxLength(10)]
        public string TermEamal { get; set; } = "";

        // ======== Navigation Properties ========

        // گروه آموزشی
        [ForeignKey(nameof(GrooheAmoozeshiId))]
        public GrooheAmoozeshi? GrooheAmoozeshi { get; set; }

        // Navigation Properties
        public ICollection<Daneshjoo>? Daneshjoos { get; set; }
    }
}