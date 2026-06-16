using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Core
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
        [MaxLength(10)]
        public string? CodeMelli { get; set; }

        /// <summary>نام</summary>
        [MaxLength(100)]
        public string? Naam { get; set; }

        /// <summary>نام خانوادگی</summary>
        [MaxLength(100)]
        public string? NaameKhanevadeghi { get; set; }

        /// <summary>تلفن مستقیم</summary>
        [MaxLength(20)]
        public string? TelefonMostaghim { get; set; }

        /// <summary>تلفن غیر مستقیم</summary>
        [MaxLength(20)]
        public string? TelefonGhayreMostaghim { get; set; }

        /// <summary>تلفن داخلی</summary>
        [MaxLength(10)]
        public string? TelefonDakheli { get; set; }

        /// <summary>تلفن همراه ۱</summary>
        [MaxLength(20)]
        public string? Mobile { get; set; }

        /// <summary>تلفن همراه ۲</summary>
        [MaxLength(20)]
        public string? Mobile2 { get; set; }

        /// <summary>ایمیل</summary>
        [MaxLength(200)]
        public string? Email { get; set; }

        /// <summary>آدرس</summary>
        [MaxLength(500)]
        public string? Adres { get; set; }

        /// <summary>کد پستی</summary>
        [MaxLength(20)]
        public string? CodePosti { get; set; }
    }
}