namespace PayamBack.DTOs.Core.Karmand
{
    public class KarmandDetailDto
    {
        public int Id { get; set; }
        public string CodeMelli { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaameKhanevadeghi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public int MarkazAsliId { get; set; }
        public string MarkazAsliName { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Mobile2 { get; set; } = string.Empty;
        public string TelefonMostaghim { get; set; } = string.Empty;
        public string TelefonGhayreMostaghim { get; set; } = string.Empty;
        public string TelefonDakheli { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Emza { get; set; } = string.Empty;
    }
}