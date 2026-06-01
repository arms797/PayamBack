using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// رشته‌های تحصیلی
    /// </summary>
    public class Reshteh
    {
        // ======== شناسه فنی (توصیه‌شده) ========
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ======== کدهای شناسایی ========
        /// <summary>کد مقطع</summary>
        [Required]
        [MaxLength(50)]
        public string CodeMaghta { get; set; }

        /// <summary>مقطع تحصیلی (کارشناسی، ارشد، دکتری)</summary>
        [Required]
        [MaxLength(100)]
        public string Maghta { get; set; }

        /// <summary>کد دانشکده</summary>
        [Required]
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; }

        /// <summary>کد گروه آموزشی</summary>
        [Required]
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; }

        /// <summary>کد دو رقمی رشته</summary>
        [Required]
        [MaxLength(50)]
        public string CodeReshteDoRaghami { get; set; }

        /// <summary>کد رشته</summary>
        [Required]
        [MaxLength(50)]
        public string CodeReshte { get; set; }

        // ======== اطلاعات ترم ========
        /// <summary>ترم ورود</summary>
        [Required]
        [MaxLength(10)]
        public string TermVorood { get; set; }

        /// <summary>ترم اعمال</summary>
        [Required]
        [MaxLength(10)]
        public string TermEamal { get; set; }

        // ======== عنوان و ترکیب کد ========
        /// <summary>عنوان رشته</summary>
        [Required]
        [MaxLength(200)]
        public string OnvanReshte { get; set; }

        /// <summary>ترکیب کد (کد کامل رشته)</summary>
        [Required]
        [MaxLength(100)]
        public string TarkibeCode { get; set; }
    }
}
