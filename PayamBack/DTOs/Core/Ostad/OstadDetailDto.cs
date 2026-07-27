namespace PayamBack.DTOs.Core.Ostad
{
    public class OstadDetailDto
    {
        public int Id { get; set; }
        public string CodeOstadi { get; set; } = string.Empty;
        public string Naam { get; set; } = string.Empty;
        public string NaamKhanevadegi { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public int MarkazAsliId { get; set; }
        public string MarkazAsliName { get; set; } = string.Empty;
        public string Jens { get; set; } = string.Empty;
        public string NaamPedar { get; set; } = string.Empty;
        public string TarikhTavalod { get; set; } = string.Empty;
        public string ShomareShenasname { get; set; } = string.Empty;
        public string ShomareMelli { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Mobile { get; set; } = string.Empty;
        public string Mobile2 { get; set; } = string.Empty;
        public string MartabeElmi { get; set; } = string.Empty;
        public string SazmanMarboote { get; set; } = string.Empty;
        public string MahalEshteghal { get; set; } = string.Empty;
        public string Emza { get; set; } = string.Empty;
        public bool Vazeeat {  get; set; }
        public bool VazeeatMovaghat { get; set; }
        public int NoeHamkari { get; set; }
        public string NoeBimeh { get; set; } = string.Empty;
        public string ShomarehBimeh { get; set; } = string.Empty;
    }
}