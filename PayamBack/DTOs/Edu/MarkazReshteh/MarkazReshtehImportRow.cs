namespace PayamBack.DTOs.Edu.MarkazReshteh
{
    public class MarkazReshtehImportRow
    {
        public int RowNumber { get; set; }
        public string? CodeOstan { get; set; }       // ستون 1
        public string? CodeMarkaz { get; set; }      // ستون 2
        public string? CodeMaghta { get; set; }      // ستون 3
        public string? CodeReshte { get; set; }      // ستون 4
        public string? TermVorood { get; set; }      // ستون 5
        public string? TermEamal { get; set; }       // ستون 6
        public int? TedadDaneshjoo { get; set; }     // ستون 7
    }
}