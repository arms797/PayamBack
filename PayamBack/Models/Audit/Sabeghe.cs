using PayamBack.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Audit
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
        
        public DateTime? ZamanLogin { get; set; }

        // نام جدول
        [ MaxLength(100)]
        public string? Table { get; set; } 

        // نام کاربر
        [Required]
        public int UserId { get; set; } 

        // شناسه رکورد تغییر دهنده
        [ MaxLength(100)]
        public string? IdRecordTagirDahande { get; set; }

        // روز هفته
        [ MaxLength(20)]
        public string? RoozHafte { get; set; } 

        // زمان تغییر        
        public DateTime? ZamanTagir { get; set; }

        // توضیح تغییرات
        [ MaxLength(1000)]
        public string? TozihTagirat { get; set; } 

        // زمان لاگ‌اوت (اگر null باشد یعنی هنوز خارج نشده)
        public DateTime? ZamanLogOut { get; set; }

        // Navigation Property به AppUser
        [ForeignKey(nameof(UserId))]
        public AppUser? User { get; set; }
    }
}