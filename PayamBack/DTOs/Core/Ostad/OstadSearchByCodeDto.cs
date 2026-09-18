namespace PayamBack.DTOs.Ostad
{
    public class OstadSearchByCodeDto
    {
        public int Id { get; set; }
        public string CodeOstadi { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaamKhanevadegi { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public int? MarkazId { get; set; }
        public string? MarkazName { get; set; }
        public int? MarkazAsliId { get; set; }
        public string? MarkazAsliName { get; set; }
    }
}