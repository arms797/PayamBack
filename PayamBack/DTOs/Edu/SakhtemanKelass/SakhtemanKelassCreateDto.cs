using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.SakhtemanKelass
{
    public class SakhtemanKelassCreateDto
    {
        [Required(ErrorMessage = "شناسه مرکز الزامی است")]
        public int MarkazId { get; set; }

        [Required(ErrorMessage = "کد ساختمان الزامی است")]
        public int CodeSakhteman { get; set; }

        [Required(ErrorMessage = "نام ساختمان الزامی است")]
        [StringLength(100)]
        public string NaamSakhteman { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد کلاس الزامی است")]
        public int CodeClass { get; set; }

        [Required(ErrorMessage = "نام کلاس الزامی است")]
        [StringLength(100)]
        public string NaamClass { get; set; } = string.Empty;

        [StringLength(50)]
        public string? NoeClass { get; set; }

        public bool Vazeeyat { get; set; } = true;

        public int? Zarfiat { get; set; }
        public int? Tabagheh { get; set; }
        public int? ZarfiatEmtahani { get; set; }
        public int? TedadWhiteboard { get; set; }
        public bool Projector { get; set; } = false;

        [StringLength(500)]
        public string? Emkanat { get; set; }

        [StringLength(500)]
        public string? Tozihat { get; set; }

        [StringLength(20)]
        public string? Telephon { get; set; }
    }
}