namespace PayamBack.DTOs.Core.Markaz
{
    public class MarkazListDto
    {
        public int Id { get; set; }
        public string CodeMarkaz { get; set; } = string.Empty;
        public string NaamMarkaz { get; set; } = string.Empty;
        public string CodeOstan { get; set; } = string.Empty;
        public string NaamOstan { get; set; } = string.Empty;
        public bool Vazeeyat { get; set; }
    }
}