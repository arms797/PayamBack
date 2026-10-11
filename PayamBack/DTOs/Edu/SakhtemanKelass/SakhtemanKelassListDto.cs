namespace PayamBack.DTOs.Edu.SakhtemanKelass
{
    public class SakhtemanKelassListDto
    {
        public int Id { get; set; }
        public int MarkazId { get; set; }
        public string? MarkazName { get; set; }
        public string NaamSakhteman { get; set; } = string.Empty;
        public string NaamClass { get; set; } = string.Empty;
        public bool? Amoozeshi { get; set; }
        public bool Vazeeyat { get; set; }
        public bool Projector { get; set; }
        public int? TedadWhiteboard { get; set; }
    }
}