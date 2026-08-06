namespace PayamBack.DTOs.Schedule.Faaliat
{
    public class FaaliatDetailDto
    {
        public int Id { get; set; }
        public string Onvan { get; set; } = string.Empty;
        public int NoeAnjam { get; set; }
        public string? NoeAnjamDisplay { get; set; }
        public int? MinSaatDarEdari { get; set; }
        public int? MaxSaatDarEdari { get; set; }
        public int? MinSaatDarHafteh { get; set; }
        public int? MaxSaatDarHafteh { get; set; }
        public int? MinDayDarHafteh { get; set; }
        public int? MaxDayDarHafteh { get; set; }
        public bool? IsMadove { get; set; }
        public string? Color { get; set; }
        public bool? Vazeeat { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}