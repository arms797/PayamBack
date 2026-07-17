namespace PayamBack.DTOs.Core.Daneshjoo
{
    public class DaneshjooListDto
    {
        public int Id { get; set; }
        public string ShomareDaneshjooee { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaamKhanevadegi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public int ReshtehId { get; set; }
        public string ReshtehName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}