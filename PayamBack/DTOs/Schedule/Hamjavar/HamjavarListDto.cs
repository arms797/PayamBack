namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarListDto
    {
        public int Id { get; set; }

        // ============================================================
        // اطلاعات پایه
        // ============================================================
        public int OstadId { get; set; }
        public string? OstadName { get; set; }
        public string? OstadCode { get; set; }
        public string? MarkazName { get; set; }
        public string? CodeTerm { get; set; }
        public string? TermName { get; set; }

        // ============================================================
        // اطلاعات تدریس
        // ============================================================
        public decimal? VahedMahalKhedmat { get; set; }
        public decimal? VahedHamjavar { get; set; }
        public decimal? VahedMajazi { get; set; }
        public string? Dalil { get; set; }
        public string? ShahrZendegi { get; set; }

        // ============================================================
        // مرحله 1: عضو علمی
        // ============================================================
        public string? UploadElmi { get; set; }
        public int? AmaliatElmi { get; set; }
        public string? NazarElmi { get; set; }
        public DateTime? TarikhErsalElmi { get; set; }

        // ============================================================
        // مرحله 2: رئیس مرکز
        // ============================================================
        public DateTime? TarikhDaryaftRaeis { get; set; }
        public string? TozihatRaeis { get; set; }
        public string? UploadRaeis { get; set; }
        public int? AmaliatRaeis { get; set; }
        public string? NazarRaeis { get; set; }
        public DateTime? TarikhErsalRaeis { get; set; }

        // ============================================================
        // مرحله 3: مدیر خدمات آموزشی استان
        // ============================================================
        public DateTime? TarikhDaryaftKhadamat { get; set; }
        public string? TozihatKhadamat { get; set; }
        public string? UploadKhadamat { get; set; }
        public int? AmaliatKhadamat { get; set; }
        public string? NazarKhadamat { get; set; }
        public DateTime? TarikhErsalKhadamat { get; set; }

        // ============================================================
        // مرحله 4: معاونت آموزشی استان
        // ============================================================
        public DateTime? TarikhDaryaftMoaven { get; set; }
        public string? TozihatMoaven { get; set; }
        public string? UploadMoaven { get; set; }
        public int? AmaliatMoaven { get; set; }
        public string? NazarMoaven { get; set; }
        public DateTime? TarikhErsalMoaven { get; set; }

        // ============================================================
        // وضعیت نهایی
        // ============================================================
        public string? KharinBarrasi { get; set; }
        public string? AkharinTaghaza { get; set; }
        public DateTime? LastUpdate { get; set; }
    }
}