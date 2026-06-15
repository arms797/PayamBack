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

        // کد ملی
        [Required, MaxLength(10)]
        public string CodeMelli { get; set; } = "";

        // نام
        [Required, MaxLength(100)]
        public string Naam { get; set; } = "";

        // نام خانوادگی
        [Required, MaxLength(100)]
        public string NaameKhanevadeghi { get; set; } = "";

        // تلفن مستقیم
        [MaxLength(20)]
        public string? TelefonMostaghim { get; set; }

        // تلفن غیر مستقیم
        [MaxLength(20)]
        public string? TelefonGhayreMostaghim { get; set; }

        // تلفن داخلی
        [MaxLength(10)]
        public string? TelefonDakheli { get; set; }

        // تلفن همراه ۱
        [MaxLength(20)]
        public string? Mobile { get; set; }

        // تلفن همراه ۲
        [MaxLength(20)]
        public string? Mobile2 { get; set; }

        // ایمیل
        [MaxLength(200)]
        public string? Email { get; set; }

        // آدرس
        [MaxLength(500)]
        public string? Adres { get; set; }

        // کد پستی
        [MaxLength(20)]
        public string? CodePosti { get; set; }
    }
}