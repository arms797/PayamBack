namespace PayamBack.DTOs.Core.Markaz
{
    public class MarkazUpdateDto
    {
        public string? NaamMarkaz { get; set; }
        public string? CodeOstan { get; set; }
        public string? NaamOstan { get; set; }
        public string? VahedMarkaz { get; set; }
        public string? Nahiyeh { get; set; }
        public string? MahalMarkaz { get; set; }
        public string? Adres { get; set; }
        public string? CodePosti { get; set; }
        public string? WebSite { get; set; }
        public string? Telefon { get; set; }
        public bool? Vazeeyat { get; set; }
        public bool? Dakheli { get; set; }
        public int? Level { get; set; }  // ← اضافه شد
        public int? NoeMarkaz {  get; set; }
    }
}