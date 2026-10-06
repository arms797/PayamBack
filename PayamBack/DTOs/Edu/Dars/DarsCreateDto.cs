using PayamBack.DTOs.Edu.ManbaDars;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.Dars
{
    public class DarsCreateDto
    {
        [Required(ErrorMessage = "کد درس الزامی است")]
        [StringLength(20)]
        public string CodeDars { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام درس الزامی است")]
        [StringLength(200)]
        public string NaamDars { get; set; } = string.Empty;

        public decimal? VahedTeori { get; set; }
        public decimal? VahedAmali { get; set; }
        public int? SaatTeoriOrginal { get; set; }
        public int? SaatAmaliOrginal { get; set; }
        public int? SaatTeori { get; set; }
        public int? SaatAmali { get; set; }
        public int? TermAkhz { get; set; }

        [StringLength(50)]
        public string? NoeDars { get; set; }

        [StringLength(50)]
        public string? NoeAzmoon { get; set; }

        public int? ReshtehId { get; set; }
        public int? Zarfiat { get; set; }

        // 🔥 لیست منابع
        public List<ManbaDarsCreateDto> ManbaList { get; set; } = new();
    }
}