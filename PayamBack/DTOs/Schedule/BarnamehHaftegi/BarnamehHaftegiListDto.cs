namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BarnamehHaftegiListDto
    {
        // اطلاعات استاد
        public int OstadId { get; set; }
        public string OstadName { get; set; } = string.Empty;
        public string OstadCode { get; set; } = string.Empty;
        public string OstadMarkaz { get; set; } = string.Empty;
        public int NoeHamkari { get; set; }
        public string? MartabeElmi { get; set; }
        public int? Maghta { get; set; }
        public string? Reshteh { get; set; }
        public int? GrooheAmoozeshiId { get; set; }

        // وضعیت برنامه
        public bool HasProgram { get; set; }
        public string ApproveStatus { get; set; } = string.Empty;  // pishnevis, tayeed_ostad, tayeed_modir, tayeed_moaven, no_program
        public string ApproveStatusDisplay { get; set; } = string.Empty;

        // اطلاعات برنامه (در صورت وجود)
        public int? ProgramId { get; set; }
        public int? NazarElmi { get; set; }
        public int? NazarModirGrooh { get; set; }
        public int? NazarMoaven { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? CreatedAt { get; set; }
    }
}
