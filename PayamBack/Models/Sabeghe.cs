using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    // تاریخچه فعالیت‌ها و لاگ‌های سیستم
    public class Sabeghe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // IP دستگاه
        [Required, MaxLength(50)]
        public string IpSystem { get; set; } = "";

        // نام دستگاه
        [Required, MaxLength(200)]
        public string Dastgah { get; set; } = "";

        // مرورگر
        [Required, MaxLength(200)]
        public string Moroorgar { get; set; } = "";

        // زمان لاگین
        [Required]
        public DateTime? ZamanLogin { get; set; }

        // نام جدول
        [Required, MaxLength(100)]
        public string Table { get; set; } = "";

        // نام کاربر
        [Required, MaxLength(100)]
        public string User { get; set; } = "";

        // شناسه رکورد تغییر دهنده
        [Required, MaxLength(100)]
        public string IdRecordTagirDahande { get; set; } = "";

        // روز هفته
        [Required, MaxLength(20)]
        public string RoozHafte { get; set; } = "";

        // زمان تغییر
        [Required]
        public DateTime ZamanTagir { get; set; }

        // توضیح تغییرات
        [Required, MaxLength(1000)]
        public string TozihTagirat { get; set; } = "";

        // زمان لاگ‌اوت (اگر null باشد یعنی هنوز خارج نشده)
        public DateTime? ZamanLogOut { get; set; }
    }
}