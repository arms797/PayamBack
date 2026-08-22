namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermListDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? OstadName { get; set; }
        public string? OstadCode { get; set; }
        public string? OstadMarkaz { get; set; }
        public string? AkharinVazeeat { get; set; }
        public bool? IsEjeari { get; set; }
        public string? OnvanEjraei { get; set; }
        public bool? FullTime { get; set; }
        public int? TedadSaatMovazafi { get; set; }
        public decimal? TedadVahedMovazafi { get; set; }
        public bool Vazeeat { get; set; }
        public int? ApproveStatus { get; set; }
        public string? ApproveStatusDisplay { get; set; }
        public string? ApprovedBy { get; set; }
        public bool HasFile { get; set; }
        public string? FilePath { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}