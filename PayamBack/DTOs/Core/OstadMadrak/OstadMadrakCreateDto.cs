using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Core.OstadMadrak
{
    public class OstadMadrakCreateDto
    {
        [Required]
        public int OstadId { get; set; }

        [Required]
        public string Reshteh { get; set; } = string.Empty;

        public string? Grayesh { get; set; }

        [Required]
        public int Maghta { get; set; }

        public bool? PishFarz { get; set; }

        public string? MahalAkhz { get; set; }

        public string? TasvirMadrak { get; set; }

        public int? GrooheAmoozeshiId { get; set; }
    }
}