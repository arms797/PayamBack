namespace PayamBack.DTOs.Core.Markaz
{
    public class MarkazDetailDto
    {
        public int Id { get; set; }
        public string CodeMarkaz { get; set; } = string.Empty;
        public string NaamMarkaz { get; set; } = string.Empty;
        public string CodeOstan { get; set; } = string.Empty;
        public string NaamOstan { get; set; } = string.Empty;
        public string VahedMarkaz { get; set; } = string.Empty;
        public string Nahiyeh { get; set; } = string.Empty;
        public string MahalMarkaz { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string CodePosti { get; set; } = string.Empty;
        public string WebSite { get; set; } = string.Empty;
        public string Telefon { get; set; } = string.Empty;
        public bool Vazeeyat { get; set; }
    }
}