using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models
{
    public class BarnamehHaftegiOstad
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(50)]
        public string CodeOstad { get; set; } = "";

        [Required, MaxLength(50)]
        public string CodeOstan { get; set; } = "";

        [Required, MaxLength(50)]
        public string CodeMarkaz { get; set; } = "";

        [Required, MaxLength(50)]
        public string CodeTerm { get; set; } = "";

        [Required, MaxLength(50)]
        public string RoozeHafteh { get; set; } = "";   // مثلاً "شنبه" یا "Saturday"

        [MaxLength(200)]
        public string? A { get; set; }

        [MaxLength(200)]
        public string? B { get; set; }

        [MaxLength(200)]
        public string? C { get; set; }

        [MaxLength(200)]
        public string? D { get; set; }

        [MaxLength(200)]
        public string? E { get; set; }

        [MaxLength(200)]
        public string? F { get; set; }

        [MaxLength(200)]
        public string? G { get; set; }

        [MaxLength(200)]
        public string? H { get; set; }

        public bool Jozeiat { get; set; } = false;

        [MaxLength(500)]
        public string? Tozihat { get; set; }
    }
}
