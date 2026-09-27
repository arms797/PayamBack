namespace PayamBack.DTOs.Schedule.DarsEraeh
{
    public class DarsEraehListDto
    {
        public int Id { get; set; }
        public string CodeTerm { get; set; } = string.Empty;
        public int MarkazId { get; set; }
        public int ReshtehId { get; set; }
        public int DarsId { get; set; }
        public string CodeDars { get; set; } = string.Empty;
        public int Grooh { get; set; }
        public string? NoeTadris { get; set; }
        public int? UserIdMojavezDahandeh { get; set; }
        public string? NaghshMarkazMojavezDahandeh { get; set; }
        public DateTime? TarikheEraheh { get; set; }
        public int? Jensiat { get; set; }
        public bool VazeeyatDars { get; set; }
        public int? NahvehEraehDars { get; set; }
        public bool EmkanAkhzSayerMarakez { get; set; }
        public bool EmkanLinkBeSayerMarakez { get; set; }
        public int? ErtebatDehiId { get; set; }
        public string? BarnamehRizi { get; set; }
        public int? SabtNami { get; set; }
    }
}