using PayamBack.Models.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Core.Karmand
{
    public class KarmandCreateDto
    {
        [Required]
        public string CodeMelli { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Naam { get; set; } = string.Empty;

        [Required]
        public string NaameKhanevadeghi { get; set; } = string.Empty;

        [Required]
        public int MarkazId { get; set; }

        public int? MarkazAsliId { get; set; }

        public string? Mobile { get; set; }
        public string? Mobile2 { get; set; }
        public string? TelefonMostaghim { get; set; }
        public string? TelefonGhayreMostaghim { get; set; }
        public string? TelefonDakheli { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Emza { get; set; }

        public int? RoleId { get; set; }
    }
}