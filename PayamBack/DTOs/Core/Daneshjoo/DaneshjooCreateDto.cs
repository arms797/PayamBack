using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Core.Daneshjoo
{
    public class DaneshjooCreateDto
    {
        [Required]
        public string ShomareDaneshjooee { get; set; } = string.Empty;

        [Required]
        public string Naam { get; set; } = string.Empty;

        [Required]
        public string NaamKhanevadegi { get; set; } = string.Empty;

        [Required]
        public string ShomareMelli { get; set; } = string.Empty;

        [Required]
        public int MarkazId { get; set; }

        public int? MarkazAzmoonId { get; set; }
        public int? MarkazTermiId { get; set; }
        public int? ReshtehId { get; set; }

        public string? Jens { get; set; }
        public string? Naampedar { get; set; }
        public string? ShomareShenasname { get; set; }
        public DateOnly? TarikhTavalod { get; set; }
        public string? TermVorood { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Mobile { get; set; }

        public string? RoleName { get; set; }
    }
}