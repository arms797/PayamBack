using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarUpdateDto
    {
        [Required]
        public int Id { get; set; }

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

        [MaxLength(1000)]
        public string? NazarElmi { get; set; }

        /// <summary>لیست جزئیات تکمیلی برای ویرایش</summary>
        public List<Hamjavar1UpdateDto>? Hamjavar1s { get; set; }
    }

    public class Hamjavar1UpdateDto
    {
        [Required]
        public int Id { get; set; }

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