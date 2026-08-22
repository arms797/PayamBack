// DTOs/Schedule/Hamjavar/HamjavarUpdateDto.cs
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarUpdateDto
    {
        public decimal? VahedMovazaf { get; set; }
        public decimal? TedadVahedMahalKhedmat { get; set; }
        public decimal? TedadVahedHamjavar { get; set; }
        public decimal? TedadVahedMajazi { get; set; }

        [MaxLength(500)]
        public string? Dalil { get; set; }

        [MaxLength(200)]
        public string? ShahrZendegi { get; set; }

        public IFormFile? UploadElmi { get; set; }

        /// <summary>لیست جزئیات تکمیلی برای ویرایش</summary>
        public string? Hamjavar1sJson { get; set; }
    }

    public class Hamjavar1UpdateDto
    {
        public int Id { get; set; }  // 0 برای موارد جدید

        public int? MarkazId { get; set; }
        public bool? InOstan { get; set; }

        /// <summary>
        /// لیست شناسه فعالیت‌ها
        /// </summary>
        public List<int>? FaaliatIds { get; set; }

        public int? TedadRoozElmi { get; set; }
        public int? TedadRoozRaeis { get; set; }
        public int? TedadRoozKhadamat { get; set; }
        public int? TedadRoozMoaven { get; set; }
    }
}