namespace PayamBack.DTOs.Edu.MarkazReshteh
{
    public class MarkazReshtehFilteredItemDto
    {
        public int ReshtehId { get; set; }
        public string? CodeReshte { get; set; }
        public string? OnvanReshte { get; set; }
        public string? CodeMaghta { get; set; }
        public string? Maghta { get; set; }
        public int? GrooheAmoozeshiId { get; set; }
        public string? GrooheAmoozeshiName { get; set; }
        public string? CodeDaneshkadeh { get; set; }
        public string? DaneshkadehName { get; set; }
        public int? CalculatedTedad { get; set; }
        public bool AlreadyAssigned { get; set; }
        public int? ExistingMarkazReshtehId { get; set; }
    }
}