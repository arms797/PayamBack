using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Core.Ostad
{
    public class OstadCreateDto
    {
        [Required]
        public string CodeOstadi { get; set; } = string.Empty;

        [Required]
        public string Naam { get; set; } = string.Empty;

        [Required]
        public string NaamKhanevadegi { get; set; } = string.Empty;

        [Required]
        public string ShomareMelli { get; set; } = string.Empty;

        [Required]
        public int MarkazId { get; set; }

        public int? MarkazAsliId { get; set; }

        public string? Jens { get; set; }
        public string? NaamPedar { get; set; }
        public string? TarikhTavalod { get; set; }
        public string? ShomareShenasname { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Mobile { get; set; }
        public string? Mobile2 { get; set; }

        public string? MartabeElmi { get; set; }
        public string? SazmanMarboote { get; set; }
        public string? MahalEshteghal { get; set; }
        public string? Emza { get; set; }

        public int? NoeHamkari { get; set; }
        public string? NoeBimeh { get; set; }
        public string? ShomarehBimeh { get; set; }

        public string? RoleName { get; set; }
    }
}