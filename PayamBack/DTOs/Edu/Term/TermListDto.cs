namespace PayamBack.DTOs.Edu.Term
{
    public class TermListDto
    {
        public string CodeTerm { get; set; } = string.Empty;
        public string OnvanTerm { get; set; } = string.Empty;
        public DateOnly? TermJari { get; set; }
        public bool Vazeeyat { get; set; }
    }
}