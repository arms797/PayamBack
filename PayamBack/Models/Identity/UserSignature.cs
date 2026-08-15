using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Identity
{
    /// <summary>
    /// امضای دیجیتال کاربران
    /// </summary>
    public class UserSignature
    {
        [Key]
        public int Id { get; set; }

        /// <summary>شناسه کاربر</summary>
        public int UserId { get; set; }

        /// <summary>داده‌های امضا (Base64)</summary>
        [MaxLength(5000)]
        public string Signature { get; set; } = string.Empty;

        /// <summary>موقعیت متن روی امضا (TL, TC, TR, ML, MC, MR, BL, BC, BR, ABOVE, BELOW)</summary>
        [MaxLength(50)]
        public string? Position { get; set; } = "BC";

        /// <summary>آیا کاربر اجازه ویرایش امضا را دارد؟</summary>
        public bool CanEditSignature { get; set; } = false;
        /// <summary>آیا کاربر اجازه ویرایش موقعیت را دارد؟</summary>
        public bool CanEditPosition { get; set; } = false;

        /// <summary>تاریخ ایجاد امضا</summary>
        public DateTime? CreatedAt { get; set; } = DateTime.UtcNow;

        /// <summary>تاریخ آخرین بروزرسانی</summary>
        public DateTime? UpdatedAt { get; set; }

        // ======== Navigation ========
        [ForeignKey(nameof(UserId))]
        public virtual AppUser? User { get; set; }
    }
}
