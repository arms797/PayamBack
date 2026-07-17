using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Core.Admin
{
    public class AdminCreateDto
    {
        [Required]
        public string CodeMelli { get; set; } = string.Empty;

        [Required]
        public string UserName { get; set; } = string.Empty;

        [Required]
        public string Naam { get; set; } = string.Empty;

        [Required]
        public string NaameKhanevadeghi { get; set; } = string.Empty;

        public string? TelefonMostaghim { get; set; }
        public string? TelefonGhayreMostaghim { get; set; }
        public string? TelefonDakheli { get; set; }

        public string? Mobile { get; set; }
        public string? Mobile2 { get; set; }

        [EmailAddress]
        public string? Email { get; set; }

        public string? Adres { get; set; }
        public string? CodePosti { get; set; }

        public string? RoleName { get; set; }
    }
}