
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarReviewDto
    {
        [Required]
        public int HamjavarId { get; set; }

        /// <summary>
        /// تعداد روز پیشنهادی برای هر Hamjavar1
        /// ترتیب این لیست باید با ترتیب Hamjavar1 ها مطابقت داشته باشد
        /// </summary>
        public List<int>? TedadRoozList { get; set; }

        [MaxLength(1000)]
        public string? Nazar { get; set; }

        [MaxLength(500)]
        public string? Upload { get; set; }
    }
}