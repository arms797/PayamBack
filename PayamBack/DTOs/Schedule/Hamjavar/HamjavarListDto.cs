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

        /// <summary>وضعیت نهایی (کد)</summary>
        public string AkharinTaghaza { get; set; } = string.Empty;

        /// <summary>وضعیت نهایی (نمایشی)</summary>
        public string AkharinTaghazaDisplay { get; set; } = string.Empty;

        /// <summary>آخرین مرحله بررسی شده</summary>
        public string KharinBarrasi { get; set; } = string.Empty;

        /// <summary>آیا Hamjavar1 دارد؟</summary>
        public bool HasHamjavar1s { get; set; }

        public DateTime? CreatedAt { get; set; }
    }
}