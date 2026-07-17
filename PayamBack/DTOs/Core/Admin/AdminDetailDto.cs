namespace PayamBack.DTOs.Core.Admin
{
    public class AdminDetailDto
    {
        public int Id { get; set; }
        public string CodeMelli { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaameKhanevadeghi { get; set; } = string.Empty;
        public string TelefonMostaghim { get; set; } = string.Empty;
        public string TelefonGhayreMostaghim { get; set; } = string.Empty;
        public string TelefonDakheli { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Mobile2 { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Adres { get; set; } = string.Empty;
        public string CodePosti { get; set; } = string.Empty;
    }
}