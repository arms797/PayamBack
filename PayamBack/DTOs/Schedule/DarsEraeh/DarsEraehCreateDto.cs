using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.DarsEraeh
{
    public class DarsEraehCreateDto
    {
        [Required(ErrorMessage = "کد ترم الزامی است")]
        [StringLength(20)]
        public string CodeTerm { get; set; } = string.Empty;

        [Required(ErrorMessage = "مرکز الزامی است")]
        public int MarkazId { get; set; }

        [Required(ErrorMessage = "رشته الزامی است")]
        public int ReshtehId { get; set; }

        [Required(ErrorMessage = "درس الزامی است")]
        public int DarsId { get; set; }
        [Required(ErrorMessage = "کد درس الزامی است")]

        [StringLength(20)]
        public string CodeDars { get; set; } = string.Empty;

        [Required(ErrorMessage = "شماره گروه الزامی است")]
        public int Grooh { get; set; }

        [StringLength(100)]
        public string? NoeTadris { get; set; }

        public int? UserIdMojavezDahandeh { get; set; }

        [StringLength(100)]
        public string? NaghshMarkazMojavezDahandeh { get; set; }

        public DateTime? TarikheEraheh { get; set; }

        public int? Jensiat { get; set; }

        public bool VazeeyatDars { get; set; } = true;

        public int? NahvehEraehDars { get; set; }

        public bool EmkanAkhzSayerMarakez { get; set; } = false;

        public bool EmkanLinkBeSayerMarakez { get; set; } = false;

        public int? ErtebatDehiId { get; set; }

        [StringLength(100)]
        public string? BarnamehRizi { get; set; }

        public int? SabtNami { get; set; }
    }
}