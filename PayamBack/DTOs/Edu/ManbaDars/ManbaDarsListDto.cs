namespace PayamBack.DTOs.Edu.ManbaDars
{
    public class ManbaDarsListDto
    {
        public int Id { get; set; }
        public int DarsId { get; set; }
        public string? DarsCode { get; set; }       // از Dars
        public string? DarsName { get; set; }       // از Dars
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
    }
}