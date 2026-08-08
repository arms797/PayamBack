using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarCreateDto
    {
        [Required(ErrorMessage = "شناسه استاد الزامی است")]
        public int OstadId { get; set; }

        [Required(ErrorMessage = "کد ترم الزامی است")]
        [MaxLength(50)]
        public string TermCode { get; set; } = string.Empty;

        public decimal? VahedMovazaf { get; set; }
        public decimal? TedadVahedMahalKhedmat { get; set; }
        public decimal? TedadVahedHamjavar { get; set; }
        public decimal? TedadVahedMajazi { get; set; }

        [MaxLength(500)]
        public string? Dalil { get; set; }

        [MaxLength(200)]
        public string? ShahrZendegi { get; set; }

        [MaxLength(500)]
        public string? UploadElmi { get; set; }

        /// <summary>نظر استاد هنگام تایید نهایی (اختیاری)</summary>
        [MaxLength(1000)]
        public string? NazarElmi { get; set; }

        /// <summary>لیست جزئیات تکمیلی (چندین رکورد Hamjavar1)</summary>
        public List<Hamjavar1CreateDto> Hamjavar1s { get; set; } = new();
    }

    public class Hamjavar1CreateDto
    {
        [Required(ErrorMessage = "شناسه مرکز الزامی است")]
        public int MarkazId { get; set; }

        public bool? InOstan { get; set; }

        /// <summary>
        /// لیست شناسه فعالیت‌ها (چندین فعالیت با جداکننده '|' ذخیره می‌شود)
        /// </summary>
        [Required(ErrorMessage = "حداقل یک فعالیت باید انتخاب شود")]
        public List<int> FaaliatIds { get; set; } = new();

        public int? TedadRoozElmi { get; set; }
        public int? TedadRoozRaeis { get; set; }
        public int? TedadRoozKhadamat { get; set; }
        public int? TedadRoozMoaven { get; set; }
    }
}