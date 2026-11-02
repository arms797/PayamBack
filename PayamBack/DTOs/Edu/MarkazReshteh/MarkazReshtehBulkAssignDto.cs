namespace PayamBack.DTOs.Edu.MarkazReshteh
{
    public class MarkazReshtehBulkAssignDto
    {
        public int Level { get; set; } = 4;
        public int MarkazId { get; set; }
        public List<MarkazReshtehBulkAssignItemDto> Items { get; set; } = new();
    }

    public class MarkazReshtehBulkAssignItemDto
    {
        public int ReshtehId { get; set; }
        public int? TedadDaneshjoo { get; set; }
        public bool Vazeeyat { get; set; } = true;
    }
}