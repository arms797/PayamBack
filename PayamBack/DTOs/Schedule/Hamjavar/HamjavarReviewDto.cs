using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    /// <summary>
    /// DTO برای ثبت نظر توسط رئیس مرکز، خدمات آموزشی و معاونت آموزشی
    /// </summary>
    public class HamjavarReviewDto
    {
        [Required]
        public int HamjavarId { get; set; }

        /// <summary>
        /// نظر (عددی)
        /// 0=هیچ, 1=پیش‌نویس استاد, 2=تایید, 3=رد, 4=اصلاح
        /// </summary>
        [Required]
        public int Nazar { get; set; }

        /// <summary>
        /// توضیحات تکمیلی
        /// </summary>
        [MaxLength(1000)]
        public string? Tozihat { get; set; }

        /// <summary>
        /// لیست تعداد روزهای پیشنهادی برای هر Hamjavar1
        /// </summary>
        public List<Hamjavar1TedadDto>? TedadRoozList { get; set; }

        /// <summary>
        /// فایل آپلودی (برای هر نقش جداگانه)
        /// </summary>
        public IFormFile? UploadFile { get; set; }
    }

    public class Hamjavar1TedadDto
    {
        [Required]
        public int Id { get; set; }

        public int? TedadRooz { get; set; }
    }
}