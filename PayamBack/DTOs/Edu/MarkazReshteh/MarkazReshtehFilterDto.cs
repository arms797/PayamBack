namespace PayamBack.DTOs.Edu.MarkazReshteh
{
    public class MarkazReshtehFilterDto
    {
        public int Level { get; set; } = 4;
        public int MarkazId { get; set; }
        public string[]? Maghtas { get; set; }
        public string[]? Daneshkades { get; set; }
        public int[]? GrooheAmoozeshiIds { get; set; }
        public string? Search { get; set; }
        public int Page { get; set; } = 1;
        public int PageSize { get; set; } = 10;
    }
}