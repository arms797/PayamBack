using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    public class BarnamehTermiOstad
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

        [Required]
        [Column(TypeName = "date")]
        public DateOnly Tarikh { get; set; }             // تاریخ (بدون ساعت)

        [MaxLength(200)]
        public string? A { get; set; }
        public bool TA { get; set; } = false;

        [MaxLength(200)]
        public string? B { get; set; }
        public bool TB { get; set; } = false;

        [MaxLength(200)]
        public string? C { get; set; }
        public bool TC { get; set; } = false;

        [MaxLength(200)]
        public string? D { get; set; }
        public bool TD { get; set; } = false;

        [MaxLength(200)]
        public string? E { get; set; }
        public bool TE { get; set; } = false;

        [MaxLength(200)]
        public string? F { get; set; }
        public bool TF { get; set; } = false;

        [MaxLength(200)]
        public string? G { get; set; }
        public bool TG { get; set; } = false;

        [MaxLength(200)]
        public string? H { get; set; }
        public bool TH { get; set; } = false;

        [Required]
        public bool Faal { get; set; } = true;
    }
}
