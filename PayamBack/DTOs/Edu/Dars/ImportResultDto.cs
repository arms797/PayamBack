namespace PayamBack.DTOs.Edu.Dars
{
    /// <summary>
    /// نتیجه‌ی نهایی عملیات Import.
    /// </summary>
    public class ImportResultDto
    {
        public int InsertedDars { get; set; }
        public int UpdatedDars { get; set; }
        public int SkippedDars { get; set; }

        public int InsertedManba { get; set; }
        public int UpdatedManba { get; set; }
        public int DeletedManba { get; set; }
        public int SkippedManba { get; set; }

        public int RejectedReshtehCount => RejectedReshtehs.Count;

        public List<RejectedReshtehDto> RejectedReshtehs { get; set; } = new();
        public List<ImportErrorDto> Errors { get; set; } = new();
    }

    public class RejectedReshtehDto
    {
        public string? ReshtehName { get; set; }
        public string? CodeReshte { get; set; }
        public string? CodeMaghta { get; set; }
        public string? TermVorood { get; set; }
        public string? TermEamal { get; set; }
        public string? Reason { get; set; }
    }

    public class ImportErrorDto
    {
        public int? RowNumber { get; set; }
        public string? CodeReshte { get; set; }
        public string? CodeDars { get; set; }
        public string? ShomareManba { get; set; }
        public string? ErrorMessage { get; set; }
        public string? Stage { get; set; }   // "Reshteh" | "Dars" | "Manba"
    }
}
