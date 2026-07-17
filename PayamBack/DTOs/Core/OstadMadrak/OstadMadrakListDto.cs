namespace PayamBack.DTOs.Core.OstadMadrak
{
    public class OstadMadrakListDto
    {
        public int Id { get; set; }
        public int OstadId { get; set; }
        public string Reshteh { get; set; } = string.Empty;
        public string Grayesh { get; set; } = string.Empty;
        public int Maghta { get; set; }
        public bool PishFarz { get; set; }
        public string MahalAkhz { get; set; } = string.Empty;
        public string TasvirMadrak { get; set; } = string.Empty;
        public int GrooheAmoozeshiId { get; set; }
        public string GrooheAmoozeshiName { get; set; } = string.Empty;
    }
}