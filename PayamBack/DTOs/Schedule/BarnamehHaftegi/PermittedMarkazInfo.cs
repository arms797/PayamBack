namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class PermittedMarkazInfo
    {
        public int MarkazId { get; set; }
        public bool IsMainMarkaz { get; set; }
        public int? MaxDays { get; set; }
        public List<int> AllowedFaaliatIds { get; set; } = new();
        public int NoeMarkaz { get; set; }
    }
}