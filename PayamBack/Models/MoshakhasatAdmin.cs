using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    /// <summary>
    /// مشخصات ادمین
    /// </summary>
    public class MoshakhasatAdmin
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

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

        /// <summary>تلفن مستقیم</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonMostaghim { get; set; } = "";

        /// <summary>تلفن غیر مستقیم</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonGhayreMostaghim { get; set; } = "";

        /// <summary>تلفن داخلی</summary>
        [Required]
        [MaxLength(10)]
        public string TelefonDakheli { get; set; } = "";

        /// <summary>فکس</summary>
        [Required]
        [MaxLength(20)]
        public string Fax { get; set; } = "";

        /// <summary>تلفن همراه ۱</summary>
        [Required]
        [MaxLength(20)]
        public string TelefonHamrah1 { get; set; } = "";

        /// <summary>تلفن همراه ۲</summary>
        [MaxLength(20)]
        public string TelefonHamrah2 { get; set; } = "";

        /// <summary>ایمیل</summary>
        [Required]
        [MaxLength(200)]
        public string Email { get; set; } = "";

        /// <summary>آدرس</summary>
        [Required]
        [MaxLength(500)]
        public string Adres { get; set; } = "";

        /// <summary>کد پستی</summary>
        [Required]
        [MaxLength(20)]
        public string CodePosti { get; set; } = "";
    }
}
