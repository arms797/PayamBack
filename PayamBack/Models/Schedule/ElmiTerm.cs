using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;

namespace PayamBack.Models.Schedule
{
    /// <summary>
    /// وضعیت ترمی اساتید با شرایط خاص
    /// </summary>
    public class ElmiTerm
    {
        [Key]
        public int Id { get; set; }

        // ============================================================
        // اطلاعات استاد و ترم
        // ============================================================

        /// <summary>شناسه کاربر (استاد)</summary>
        public int? UserId { get; set; }

        // ============================================================
        // اطلاعات ثبت‌کننده
        // ============================================================

        /// <summary>شناسه کاربر ثبت‌کننده</summary>
        public int? UserIdSabtKonandeh { get; set; }

        /// <summary>شناسه نقش ثبت‌کننده</summary>
        public int? RoleMarkazSabtKonandeh { get; set; }

        // ============================================================
        // وضعیت و اطلاعات شغلی
        // ============================================================

        /// <summary>آخرین وضعیت علمی مثل مامور به تحصیل یا فرصت مطالعاتی و ...</summary>
        public string? AkharinVazeeat { get; set; }

        /// <summary>دارای سمت اجرایی (true = بله)</summary>
        public bool? IsEjeari { get; set; }

        /// <summary>عنوان پست اجرایی</summary>
        [MaxLength(200)]
        public string? OnvanEjraei { get; set; }

        /// <summary>تمام وقت / پاره وقت (true = تمام وقت)</summary>
        public bool? FullTime { get; set; }

        /// <summary>تعداد ساعت معادل موظف</summary>
        public int? TedadSaatMovazafi { get; set; }

        //تعداد واحد موظفی
        public decimal? TedadVahedMovazafi { get; set; }


        // ============================================================
        // اطلاعات تایید (3 حالت)
        // ============================================================

        /// <summary>
        /// وضعیت تایید:
        /// 0 = در انتظار بررسی (پیش‌فرض)
        /// 1 = تایید شده
        /// 2 = رد شده
        /// </summary>
        public int? ApproveStatus { get; set; } = 0;

        /// <summary>شناسه کاربر تاییدکننده</summary>
        public int? ApprovedByUserId { get; set; }

        /// <summary>نقش و مرکز کاربر تاییدکننده</summary>
        /// مثلا - معاونت آموزشی-استان فارس
        public string? ApprovedByRoleMarkaz { get; set; }

        /// <summary>تاریخ تایید/رد</summary>
        public DateTime? ApprovedAt { get; set; }

        /// <summary>توضیحات تایید/رد</summary>
        [MaxLength(500)]
        public string? ApproveTozihat { get; set; }

        /// <summary>فایل مستندات </summary>
        public string? FilePath { get; set; }
        //وضعیت قابلیت اجرا
        public bool Vazeeat { get; set; } = true;

        // ============================================================
        // Navigation Properties
        // ============================================================

        [ForeignKey(nameof(UserId))]
        public virtual AppUser? User { get; set; }        

        [ForeignKey(nameof(UserIdSabtKonandeh))]
        public virtual AppUser? UserSabtKonandeh { get; set; }

        [ForeignKey(nameof(ApprovedByUserId))]
        public virtual AppUser? ApprovedByUser { get; set; }
    }
}