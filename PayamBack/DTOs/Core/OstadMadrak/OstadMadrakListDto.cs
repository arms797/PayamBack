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

    // ============================================================
    // 🔥 فیلدهای ایجاد کننده
    // ============================================================
    public int? CreatedByUserId { get; set; }
    public string? CreatedByUserInfo { get; set; }   // نام و نام خانوادگی کاربر ایجاد کننده
    public string? CreatedByRoleInfo { get; set; }   // نقش و مرکز کاربر ایجاد کننده
    public DateTime? CreatedAt { get; set; }

    // ============================================================
    // 🔥 فیلدهای تایید
    // ============================================================
    public bool IsApproved { get; set; }
    public int? ApprovedByUserId { get; set; }
    public string? ApprovedByUserInfo { get; set; }   // نام و نام خانوادگی کاربر تایید کننده
    public string? ApprovedByRoleInfo { get; set; }   // نقش و مرکز کاربر تایید کننده
    public DateTime? ApprovedAt { get; set; }
}