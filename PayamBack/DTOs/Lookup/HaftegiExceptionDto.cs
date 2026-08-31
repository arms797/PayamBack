// DTOs/Lookup/HaftegiExceptionDto.cs
namespace PayamBack.DTOs.Lookup
{
    public class HaftegiExceptionDto
    {
        public int Id { get; set; }
        public string TermCode { get; set; } = string.Empty;
        public string? OstanCode { get; set; }
        public int? DayCode { get; set; }
        public string? HourCode { get; set; }
        public int? NoeHamkariMask { get; set; }
        public List<int>? FaaliatIds { get; set; }  // ← آرایه‌ای از اعداد
        public string? Description { get; set; }
        public bool IsActive { get; set; }
    }
}