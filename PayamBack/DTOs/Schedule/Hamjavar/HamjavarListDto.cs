namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarListDto
    {
        public int Id { get; set; }
        public int OstadId { get; set; }
        public string OstadName { get; set; } = string.Empty;
        public string OstadCode { get; set; } = string.Empty;
        public string TermCode { get; set; } = string.Empty;
        public decimal VahedMovazaf { get; set; }
        public decimal TedadVahedMahalKhedmat { get; set; }
        public decimal TedadVahedHamjavar { get; set; }
        public decimal TedadVahedMajazi { get; set; }

        // وضعیت نهایی (محاسبه شده از Nazarها)
        public string AkharinTaghaza { get; set; } = string.Empty;
        public string AkharinTaghazaDisplay { get; set; } = string.Empty;
        public string AkharinBarrasi { get; set; } = string.Empty;

        public bool HasHamjavar1s { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}