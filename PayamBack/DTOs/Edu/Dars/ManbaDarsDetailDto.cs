namespace PayamBack.DTOs.Edu.Dars
{
    public class ManbaDarsDetailDto
    {
        public int Id { get; set; }
        public int DarsId { get; set; }
        public string? ShomareManba { get; set; }
        public string? NoeManba { get; set; }
        public string Onvan { get; set; } = string.Empty;
        public string? Nevisandeh { get; set; }
        public string? Motarjem { get; set; }
        public string? SalEnteshar { get; set; }
        public string? SalEntesharMiladi { get; set; }
        public string? Shabak { get; set; }
        public string? Nasher { get; set; }
        public string? NobateChap { get; set; }
        public string? Vazeeyat { get; set; }
        public string? CodePeyvast { get; set; }
        public string? SharhPeyvast { get; set; }
        public string? TermUpdate { get; set; }
        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}