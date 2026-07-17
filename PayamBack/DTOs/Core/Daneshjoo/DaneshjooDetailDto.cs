namespace PayamBack.DTOs.Core.Daneshjoo
{
    public class DaneshjooDetailDto
    {
        public int Id { get; set; }
        public string ShomareDaneshjooee { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaamKhanevadegi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public int MarkazAzmoonId { get; set; }
        public string MarkazAzmoonName { get; set; } = string.Empty;
        public int MarkazTermiId { get; set; }
        public string MarkazTermiName { get; set; } = string.Empty;
        public int ReshtehId { get; set; }
        public string ReshtehName { get; set; } = string.Empty;
        public string Jens { get; set; } = string.Empty;
        public string Naampedar { get; set; } = string.Empty;
        public string ShomareMelli { get; set; } = string.Empty;
        public string ShomareShenasname { get; set; } = string.Empty;
        public DateOnly? TarikhTavalod { get; set; }
        public string TermVorood { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
    }
}