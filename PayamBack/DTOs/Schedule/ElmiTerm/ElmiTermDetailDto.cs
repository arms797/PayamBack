namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermDetailDto
    {
        public int Id { get; set; }
        public int? UserId { get; set; }
        public string? OstadName { get; set; }
        public string? OstadCode { get; set; }
        public string? OstadMarkaz { get; set; }
        public string? TermCode { get; set; }
        public string? AkharinVazeeat { get; set; }
        public bool? IsEjeari { get; set; }
        public string? OnvanEjraei { get; set; }
        public bool? FullTime { get; set; }
        public string? TedadSaatMovazafi { get; set; }

        /// <summary>
        /// 0 = در انتظار بررسی | 1 = تایید شده | 2 = رد شده
        /// </summary>
        public int? ApproveStatus { get; set; }

        public string? ApproveStatusDisplay { get; set; }
        public string? ApprovedByUserName { get; set; }
        public string? ApprovedByRoleMarkaz { get; set; }
        public DateTime? ApprovedAt { get; set; }
        public string? ApproveTozihat { get; set; }
        public string? FilePath { get; set; }
        public string? FileName { get; set; }
        public string? CreatedBy { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}