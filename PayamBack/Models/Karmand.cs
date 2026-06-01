using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// کارمند
    /// </summary>
    public class Karmand
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>کد پرسنلی</summary>
        [Required]
        [MaxLength(50)]
        public string CodePersoneli { get; set; } = "";

        /// <summary>کد ملی</summary>
        [Required]
        [MaxLength(10)]
        public string CodeMelli { get; set; } = "";

        /// <summary>نام</summary>
        [Required]
        [MaxLength(100)]
        public string Naam { get; set; } = "";

        /// <summary>نام خانوادگی</summary>
        [Required]
        [MaxLength(100)]
        public string NaameKhanevadeghi { get; set; } = "";

        /// <summary>نام پدر</summary>
        [Required]
        [MaxLength(100)]
        public string NaamPedar { get; set; } = "";

        /// <summary>شماره شناسنامه</summary>
        [Required]
        [MaxLength(20)]
        public string ShomareShenasname { get; set; } = "";

        /// <summary>تاریخ تولد</summary>
        [Required]
        public DateTime? TarikhTavalod { get; set; }

        /// <summary>استان محل تولد</summary>
        [Required]
        [MaxLength(50)]
        public string OstanMahalTavalod { get; set; } = "";

        /// <summary>شهر محل تولد</summary>
        [Required]
        [MaxLength(50)]
        public string ShahrMahalTavalod { get; set; } = "";

        /// <summary>کد استان محل خدمت</summary>
        [Required]
        [MaxLength(50)]
        public string CodeOstan { get; set; } = "";

        /// <summary>کد مرکز محل خدمت</summary>
        [Required]
        [MaxLength(50)]
        public string CodeMarkaz { get; set; } = "";

        /// <summary>تلفن همراه ۱</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonHamrah1 { get; set; } = "";

        /// <summary>تلفن همراه ۲</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonHamrah2 { get; set; } = "";

        /// <summary>تلفن مستقیم محل کار</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonMostaghim { get; set; } = "";

        /// <summary>تلفن غیر مستقیم محل کار</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonGhayreMostaghim { get; set; } = "";

        /// <summary>شماره داخلی</summary>
        [Required]
        [MaxLength(10)]
        public string TelefonDakheli { get; set; } = "";

        /// <summary>فکس</summary>
        [Required]
        [MaxLength(20)]
        public string Fax { get; set; } = "";

        /// <summary>ایمیل</summary>
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = "";

        /// <summary>آدرس محل سکونت</summary>
        [Required]
        [MaxLength(500)]
        public string Adres { get; set; } = "";

        /// <summary>کد پستی</summary>
        [Required]
        [MaxLength(20)]
        public string CodePosti { get; set; } = "";

        /// <summary>امضا</summary>
        [MaxLength(250)]
        public string Emza { get; set; } = "";
    }
}
