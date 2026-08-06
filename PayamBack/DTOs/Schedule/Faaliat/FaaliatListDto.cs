namespace PayamBack.DTOs.Schedule.Faaliat
{
    public class FaaliatListDto
    {
        public int Id { get; set; }
        public string Onvan { get; set; } = string.Empty;
        public int NoeAnjam { get; set; }
        //public string? NoeAnjamDisplay { get; set; }
        public int? MinSaatDarHafteh { get; set; }
        public int? MaxSaatDarHafteh { get; set; }
        public bool? IsMadove { get; set; }
        public string? Color { get; set; }
        public bool? Vazeeat { get; set; }
    }
}