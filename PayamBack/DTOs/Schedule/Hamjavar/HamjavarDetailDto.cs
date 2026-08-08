namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarDetailDto
    {
        public int Id { get; set; }
        public int OstadId { get; set; }
        public string OstadName { get; set; } = string.Empty;
        public string OstadCode { get; set; } = string.Empty;
        public string OstadMarkaz { get; set; } = string.Empty;
        public string TermCode { get; set; } = string.Empty;
        public string TermName { get; set; } = string.Empty;

        // اطلاعات تدریس
        public decimal VahedMovazaf { get; set; }
        public decimal TedadVahedMahalKhedmat { get; set; }
        public decimal TedadVahedHamjavar { get; set; }
        public decimal TedadVahedMajazi { get; set; }
        public string Dalil { get; set; } = string.Empty;
        public string ShahrZendegi { get; set; } = string.Empty;

        // مرحله 1: استاد
        public string UploadElmi { get; set; } = string.Empty;
        public int? AmaliatElmi { get; set; }
        public string NazarElmi { get; set; } = string.Empty;
        public DateTime? TarikhErsalElmi { get; set; }

        // مرحله 2: رئیس مرکز
        public DateTime? TarikhDaryaftRaeis { get; set; }
        public string TozihatRaeis { get; set; } = string.Empty;
        public string UploadRaeis { get; set; } = string.Empty;
        public int? AmaliatRaeis { get; set; }
        public string NazarRaeis { get; set; } = string.Empty;
        public DateTime? TarikhErsalRaeis { get; set; }
        public int? UserIdRaeis { get; set; }
        public string RoleMarkazRaeis { get; set; } = string.Empty;

        // مرحله 3: خدمات آموزشی استان
        public DateTime? TarikhDaryaftKhadamat { get; set; }
        public string TozihatKhadamat { get; set; } = string.Empty;
        public string UploadKhadamat { get; set; } = string.Empty;
        public int? AmaliatKhadamat { get; set; }
        public string NazarKhadamat { get; set; } = string.Empty;
        public DateTime? TarikhErsalKhadamat { get; set; }
        public int? UserIdKhadamatOstan { get; set; }
        public string RoleMarkazKhadamatOstan { get; set; } = string.Empty;

        // مرحله 4: معاونت آموزشی استان
        public DateTime? TarikhDaryaftMoaven { get; set; }
        public string TozihatMoaven { get; set; } = string.Empty;
        public string UploadMoaven { get; set; } = string.Empty;
        public int? AmaliatMoaven { get; set; }
        public string NazarMoaven { get; set; } = string.Empty;
        public DateTime? TarikhErsalMoaven { get; set; }
        public int? UserIdApproved { get; set; }
        public string RoleMarkazApproved { get; set; } = string.Empty;

        // وضعیت نهایی
        public string KharinBarrasi { get; set; } = string.Empty;
        public string AkharinTaghaza { get; set; } = string.Empty;
        public string AkharinTaghazaDisplay { get; set; } = string.Empty;

        /// <summary>لیست جزئیات تکمیلی (Hamjavar1)</summary>
        public List<Hamjavar1DetailDto> Hamjavar1s { get; set; } = new();
    }

    public class Hamjavar1DetailDto
    {
        public int Id { get; set; }
        public int HamjavarId { get; set; }
        public int? MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public bool InOstan { get; set; }

        /// <summary>لیست شناسه فعالیت‌ها</summary>
        public List<int> FaaliatIds { get; set; } = new();

        /// <summary>لیست نام فعالیت‌ها (برای نمایش)</summary>
        public List<string> FaaliatNames { get; set; } = new();

        public int? TedadRoozElmi { get; set; }
        public int? TedadRoozRaeis { get; set; }
        public int? TedadRoozKhadamat { get; set; }
        public int? TedadRoozMoaven { get; set; }

        /// <summary>آیا معاون ایجاد کننده است؟</summary>
        public bool IsMoavenCreator { get; set; }
    }
}