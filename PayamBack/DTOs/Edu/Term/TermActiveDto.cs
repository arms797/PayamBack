namespace PayamBack.DTOs.Edu.Term
{
    public class TermActiveDto
    {
        public string CodeTerm { get; set; } = string.Empty;
        public string OnvanTerm { get; set; } = string.Empty;
        public DateOnly? TermJari { get; set; }
        public DateOnly? TarikheShorooClass { get; set; }
        public DateOnly? TarikhePayanClass { get; set; }
    }
}