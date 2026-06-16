using PayamBack.Models.Identity;
using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Audit
{
    /// <summary>
    /// تاریخچه فعالیت‌ها و لاگ‌های سیستم
    /// </summary>
    public class Sabeghe
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>IP دستگاه</summary>
        [MaxLength(50)]
        public string? IpSystem { get; set; }

        /// <summary>نام دستگاه</summary>
        [MaxLength(200)]
        public string? Dastgah { get; set; }

        /// <summary>مرورگر</summary>
        [MaxLength(200)]
        public string? Moroorgar { get; set; }

        /// <summary>زمان لاگین</summary>
        public DateTime? ZamanLogin { get; set; }

        /// <summary>نام جدول</summary>
        [MaxLength(100)]
        public string? Table { get; set; }

        /// <summary>شناسه کاربر</summary>
        public int? UserId { get; set; }

        /// <summary>شناسه رکورد تغییر دهنده</summary>
        [MaxLength(100)]
        public string? IdRecordTagirDahande { get; set; }

        /// <summary>روز هفته</summary>
        [MaxLength(20)]
        public string? RoozHafte { get; set; }

        /// <summary>زمان تغییر</summary>
        public DateTime? ZamanTagir { get; set; }

        /// <summary>توضیح تغییرات</summary>
        [MaxLength(1000)]
        public string? TozihTagirat { get; set; }

        /// <summary>زمان لاگ‌اوت (اگر null باشد یعنی هنوز خارج نشده)</summary>
        public DateTime? ZamanLogOut { get; set; }

        /// <summary>کاربر مرتبط</summary>
        [ForeignKey(nameof(UserId))]
        public virtual AppUser? User { get; set; }
    }
}