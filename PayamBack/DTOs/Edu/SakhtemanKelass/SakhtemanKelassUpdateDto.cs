using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.SakhtemanKelass
{
    public class SakhtemanKelassUpdateDto
    {
        public int? MarkazId { get; set; }
        public int? CodeSakhteman { get; set; }

        [StringLength(100)]
        public string? NaamSakhteman { get; set; }

        public int? CodeClass { get; set; }

        [StringLength(100)]
        public string? NaamClass { get; set; }

        [StringLength(50)]
        public string? NoeClass { get; set; }

        public bool? Vazeeyat { get; set; }

        public int? Zarfiat { get; set; }
        public int? Tabagheh { get; set; }
        public int? ZarfiatEmtahani { get; set; }
        public int? TedadWhiteboard { get; set; }
        public bool? Projector { get; set; }

        [StringLength(500)]
        public string? Emkanat { get; set; }

        [StringLength(500)]
        public string? Tozihat { get; set; }

        //[StringLength(20)]
        //public string? Telephon { get; set; }

        public bool? Amoozeshi { get; set; }
    }
}