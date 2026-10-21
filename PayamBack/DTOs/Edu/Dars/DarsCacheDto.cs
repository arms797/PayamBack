namespace PayamBack.DTOs.Edu.Dars
{
    public class DarsCacheDto
    {
        public int Id { get; set; }
        public string CodeDars { get; set; } = string.Empty;
        public string NaamDars { get; set; } = string.Empty;
        public decimal? VahedTeori { get; set; }
        public decimal? VahedAmali { get; set; }
        public int? SaatTeoriOrginal { get; set; }
        public int? SaatAmaliOrginal { get; set; }
        public int? SaatTeori { get; set; }
        public int? SaatAmali { get; set; }
        public int? TermAkhz { get; set; }
        public string? NoeDars { get; set; }
        public string? NoeAzmoon { get; set; }
        public int? ReshtehId { get; set; }
        public string? ReshtehName { get; set; }
        public string? CodeMaghta { get; set; }
        public string? Maghta { get; set; }
        public int? GrooheAmoozeshiId { get; set; }
        public string? CodeGrooheAmoozeshi { get; set; }
        public string? GrooheAoozeshiName { get; set; }
        public string? CodeDaneshkadeh { get; set; }
        public string? DaneshkadehName { get; set; }
        public int? Zarfiat { get; set; }
        public List<ManbaCacheDto> ManbaList { get; set; } = new();        
    }
    public class ManbaCacheDto
    {
        public int Id { get; set; }
        public string? ShomareManba { get; set; }
        public string Onvan { get; set; } = string.Empty;
        public string? CodePeyvast { get; set; }
    }
}