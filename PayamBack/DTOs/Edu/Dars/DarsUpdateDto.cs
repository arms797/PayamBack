using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.Dars
{
    public class DarsUpdateDto
    {
        // 🔥 همه فیلدهای DarsCreateDto رو دستی اینجا بنویس، 
        // ولی ManbaList رو با نوع UpdateDto تعریف کن

        [StringLength(20)]
        public string? CodeDars { get; set; }

        [StringLength(200)]
        public string? NaamDars { get; set; }

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

        // 🔥 لیست منابع با Id برای ویرایش
        public List<ManbaDarsUpdateDto> ManbaList { get; set; } = new();
    }
}