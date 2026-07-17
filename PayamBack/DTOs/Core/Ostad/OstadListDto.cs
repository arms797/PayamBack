namespace PayamBack.DTOs.Core.Ostad
{
    public class OstadListDto
    {
        public int Id { get; set; }
        public string CodeOstadi { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaamKhanevadegi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int NoeHamkari { get; set; }
        public bool Vazeeat { get; set; }
    }
}