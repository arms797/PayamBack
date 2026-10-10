using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BulkLockDto
    {
        /// <summary>
        /// نوع همکاری استاد (اختیاری - اگر مقدار نداشته باشد، همه اساتید شامل می‌شوند)
        /// 1=هیات علمی پیام نور، 2=هیات علمی غیر پیام نور، 3=مدرس مدعو، 4=سایر
        /// </summary>
        public int? NoeHamkari { get; set; }

        /// <summary>
        /// عملیات: "lock" برای قفل کردن، "unlock" برای باز کردن قفل
        /// </summary>
        [Required]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// کد ترم (اختیاری - اگر نباشد، ترم جاری استفاده می‌شود)
        /// </summary>
        [MaxLength(50)]
        public string? TermCode { get; set; }
    }
}
