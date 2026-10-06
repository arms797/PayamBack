namespace PayamBack.DTOs.Edu.Dars
{
    public class DarsListDto
    {
        public int Id { get; set; }
        public string CodeDars { get; set; } = string.Empty;
        public string NaamDars { get; set; } = string.Empty;
        public decimal? VahedTeori { get; set; }
        public decimal? VahedAmali { get; set; }
        public int? SaatTeoriOrginal { get; set; }
        public int? SaatAmaliOrginal { get; set; }
        public int? SaatTeori { get; set; }
        public int? SaatAmali { get; set; }
        public int? TermAkhz { get; set; }
        public string? NoeDars { get; set; }
        public string? NoeAzmoon { get; set; }
        public int? ReshtehId { get; set; }
        public string? ReshtehName { get; set; }
        public int? GrooheAmoozeshiId { get; set; }
        public string? GrooheName { get; set; }
        public int? Zarfiat { get; set; }
        public List<ManbaDarsSimpleDto> ManbaList { get; set; } = new();
        /// <summary>تعداد منابع</summary>
        public int ManbaCount { get; set; }
    }
}