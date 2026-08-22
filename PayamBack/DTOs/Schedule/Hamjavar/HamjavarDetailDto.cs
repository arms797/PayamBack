namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarDetailDto
    {
        // اطلاعات پایه Hamjavar
        public int Id { get; set; }
        public int OstadId { get; set; }
        public string? TermCode { get; set; }
        public decimal? VahedMovazaf { get; set; }
        public decimal? TedadVahedMahalKhedmat { get; set; }
        public decimal? TedadVahedHamjavar { get; set; }
        public decimal? TedadVahedMajazi { get; set; }
        public string? Dalil { get; set; }
        public string? ShahrZendegi { get; set; }
        public string? UploadElmi { get; set; }
        public string? RoleMarkazSabtKonandeh { get; set; }

        // اطلاعات استاد
        public string? OstadName { get; set; }
        public string? OstadLastName { get; set; }
        public string? OstadCode { get; set; }
        public string? OstadMarkaz { get; set; }
        public string? OstadMartabeElmi { get; set; }
        public string? OstadReshteh { get; set; }

        // اطلاعات علمی ترم
        public string? AkharinVazeeat { get; set; }
        public bool? IsEjeari { get; set; }
        public string? OnvanEjraei { get; set; }
        public bool? FullTime { get; set; }
        public int? TedadSaatMovazafi { get; set; }

        // ============================================================
        // نظرات (عددی) - مطابق مدل
        // ============================================================
        public int? NazarElmi { get; set; }
        public DateTime? TarikhErsalElmi { get; set; }

        public int? NazarRaeis { get; set; }
        public string? TozihatRaeis { get; set; }
        public string? RoleMarkazRaeis { get; set; }
        public string? RaeisFullName { get; set; }
        public string? UploadRaeis { get; set; }
        public DateTime? TarikhErsalRaeis { get; set; }

        public int? NazarKhadamat { get; set; }
        public string? TozihatKhadamat { get; set; }
        public string? RoleMarkazKhadamatOstan { get; set; }
        public string? KhadamatFullName { get; set; }
        public string? UploadKhadamat { get; set; }
        public DateTime? TarikhErsalKhadamat { get; set; }

        public int? NazarMoaven { get; set; }
        public string? TozihatMoaven { get; set; }
        public string? RoleMarkazApproved { get; set; }
        public string? MoavenFullName { get; set; }
        public string? UploadMoaven { get; set; }
        public DateTime? TarikhErsalMoaven { get; set; }

        // وضعیت نهایی (محاسبه شده)
        public string? AkharinTaghaza { get; set; }
        public string? AkharinTaghazaDisplay { get; set; }
        public string? AKharinBarrasi { get; set; }

        // Hamjavar1 ها
        public List<Hamjavar1DetailDto> Hamjavar1s { get; set; } = new();
        public SignatureDto? SignatureOstad { get; set; }
        public SignatureDto? SignatureRaeis { get; set; }
        public SignatureDto? SignatureKhadamat { get; set; }
        public SignatureDto? SignatureMoaven { get; set; }
    }

    public class Hamjavar1DetailDto
    {
        public int Id { get; set; }
        public int? MarkazId { get; set; }
        public string? MarkazName { get; set; }
        public bool? InOstan { get; set; }
        public List<int>? FaaliatIds { get; set; }
        public List<string>? FaaliatNames { get; set; }
        public int? TedadRoozElmi { get; set; }
        public int? TedadRoozRaeis { get; set; }
        public int? TedadRoozKhadamat { get; set; }
        public int? TedadRoozMoaven { get; set; }
    }
    public class SignatureDto
    {
        public string? Data { get; set; }   // Base64
        public string? Position { get; set; }
    }
}