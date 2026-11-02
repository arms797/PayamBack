namespace PayamBack.DTOs.Edu.MarkazReshteh
{
    public class MarkazReshtehBulkUploadResultDto
    {
        public int Inserted { get; set; }
        public int Updated { get; set; }
        public int Deactivated { get; set; }
        public int Skipped { get; set; }
        public int Rejected { get; set; }
        public int Total { get; set; }

        public List<RejectedMarkazReshtehRowDto> RejectedRows { get; set; } = new();
        public List<DeactivatedMarkazReshtehDto> DeactivatedRows { get; set; } = new();
    }

    public class RejectedMarkazReshtehRowDto
    {
        public int RowNumber { get; set; }
        public string? CodeOstan { get; set; }
        public string? CodeMarkaz { get; set; }
        public string? CodeMaghta { get; set; }
        public string? CodeReshte { get; set; }
        public string? TermVorood { get; set; }
        public string? TermEamal { get; set; }
        public string? Reason { get; set; }
    }

    public class DeactivatedMarkazReshtehDto
    {
        public int MarkazReshtehId { get; set; }
        public string? CodeMarkaz { get; set; }
        public string? NaamMarkaz { get; set; }
        public string? CodeReshte { get; set; }
        public string? OnvanReshte { get; set; }
        public string? CodeMaghta { get; set; }
        public string? TermVorood { get; set; }
        public string? TermEamal { get; set; }
        public int? TedadDaneshjoo { get; set; }
    }
}