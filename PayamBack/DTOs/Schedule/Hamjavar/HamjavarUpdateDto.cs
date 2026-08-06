using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarUpdateDto
    {
        [Required]
        public int Id { get; set; }

        public int? OstadId { get; set; }

        [MaxLength(50)]
        public string? CodeTerm { get; set; }

        public decimal? VahedMahalKhedmat { get; set; }
        public decimal? VahedHamjavar { get; set; }
        public decimal? VahedMajazi { get; set; }

        [MaxLength(500)]
        public string? Dalil { get; set; }

        [MaxLength(200)]
        public string? ShahrZendegi { get; set; }

        // مرحله 1: عضو علمی
        [MaxLength(500)]
        public string? UploadElmi { get; set; }

        public int? AmaliatElmi { get; set; }

        [MaxLength(1000)]
        public string? NazarElmi { get; set; }

        public DateTime? TarikhErsalElmi { get; set; }

        // مرحله 2: رئیس مرکز
        public DateTime? TarikhDaryaftRaeis { get; set; }

        [MaxLength(1000)]
        public string? TozihatRaeis { get; set; }

        [MaxLength(500)]
        public string? UploadRaeis { get; set; }

        public int? AmaliatRaeis { get; set; }

        [MaxLength(1000)]
        public string? NazarRaeis { get; set; }

        public DateTime? TarikhErsalRaeis { get; set; }

        // مرحله 3: مدیر خدمات آموزشی استان
        public DateTime? TarikhDaryaftKhadamat { get; set; }

        [MaxLength(1000)]
        public string? TozihatKhadamat { get; set; }

        [MaxLength(500)]
        public string? UploadKhadamat { get; set; }

        public int? AmaliatKhadamat { get; set; }

        [MaxLength(1000)]
        public string? NazarKhadamat { get; set; }

        public DateTime? TarikhErsalKhadamat { get; set; }

        // مرحله 4: معاونت آموزشی استان
        public DateTime? TarikhDaryaftMoaven { get; set; }

        [MaxLength(1000)]
        public string? TozihatMoaven { get; set; }

        [MaxLength(500)]
        public string? UploadMoaven { get; set; }

        public int? AmaliatMoaven { get; set; }

        [MaxLength(1000)]
        public string? NazarMoaven { get; set; }

        public DateTime? TarikhErsalMoaven { get; set; }

        // وضعیت نهایی
        [MaxLength(100)]
        public string? KharinBarrasi { get; set; }

        [MaxLength(100)]
        public string? AkharinTaghaza { get; set; }
    }
}