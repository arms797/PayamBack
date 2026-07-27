namespace PayamBack.DTOs.Core.Ostad
{
    public class OstadListDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; } 

        public string CodeOstadi { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaamKhanevadegi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;  // ← از Join با Markaz
        public int NoeHamkari { get; set; }
        public string MartabeElmi { get; set; } = string.Empty;
        public bool Vazeeat { get; set; }
        public bool VazeeatMovaghat {  get; set; }

        // ============================================================
        // 🔥 رشته تحصیلی پیش‌فرض استاد
        // ============================================================
        public string? Reshteh { get; set; }  // از OstadMadrak (PishFarz = true)
    }
}