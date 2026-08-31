// Models/Edu/HaftegiException.cs
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models.Edu
{
    public class HaftegiException
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(50)]
        public string TermCode { get; set; } = string.Empty;

        [MaxLength(50)]
        public string? OstanCode { get; set; }

        public int? DayCode { get; set; }

        [MaxLength(1)]
        public string? HourCode { get; set; }

        /// <summary>
        /// ماسک بیتی برای نوع همکاری‌های هدف
        /// 1=هیات علمی پیام نور, 2=هیات علمی غیر پیام نور, 4=مدرس مدعو, 8=سایر
        /// اگر null باشد، برای همه اعمال می‌شود.
        /// </summary>
        public int? NoeHamkariMask { get; set; }

        /// <summary>
        /// شناسه فعالیت‌های ممنوع با جداکننده '|'
        /// اگر null باشد، یعنی همه فعالیت‌ها ممنوع هستند.
        /// </summary>
        [MaxLength(50)]
        public string? FaaliatIds { get; set; }

        [MaxLength(500)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? UpdatedAt { get; set; }
    }
}