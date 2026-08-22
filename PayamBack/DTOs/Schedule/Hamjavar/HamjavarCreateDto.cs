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

        public IFormFile? UploadElmi { get; set; }

        /// <summary>
        /// لیست جزئیات تکمیلی (به صورت JSON String)
        /// </summary>
        public string? Hamjavar1sJson { get; set; }
    }

    public class Hamjavar1CreateDto
    {
        [Required(ErrorMessage = "شناسه مرکز الزامی است")]
        public int MarkazId { get; set; }
        public bool? InOstan { get; set; }

        public List<int> FaaliatIds { get; set; } = new();

        public int? TedadRoozElmi { get; set; }
        public int? TedadRoozRaeis { get; set; }
        public int? TedadRoozKhadamat { get; set; }
        public int? TedadRoozMoaven { get; set; }        
    }
}