namespace PayamBack.DTOs.Edu.SakhtemanKelass
{
    public class SakhtemanKelassListDto
    {
        public int Id { get; set; }
        public int MarkazId { get; set; }
        public string? MarkazName { get; set; }
        public int CodeSakhteman { get; set; }
        public string NaamSakhteman { get; set; } = string.Empty;
        public int CodeClass { get; set; }
        public string NaamClass { get; set; } = string.Empty;
        public string? NoeClass { get; set; }
        public bool Vazeeyat { get; set; }
        public int? Zarfiat { get; set; }
        public int? Tabagheh { get; set; }
        public int? ZarfiatEmtahani { get; set; }
        public int? TedadWhiteboard { get; set; }
        public bool? Projector { get; set; }
        public string? Emkanat { get; set; }
        public string? Tozihat { get; set; }
        public string? Telephon { get; set; }
    }
}