namespace PayamBack.DTOs.Core.Karmand
{
    public class KarmandListDto
    {
        public int Id { get; set; }
        public string CodeMelli { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaameKhanevadeghi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool Vazeeat { get; set; }  // ← اضافه شد
    }
}