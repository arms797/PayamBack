namespace PayamBack.DTOs.Identity.Menu
{
    public class MenuListDto
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public int? ParentId { get; set; }
        public string? Path { get; set; }
        public string? Icon { get; set; }
        public string? PermissionName { get; set; }
        public int Order { get; set; }
        public bool Vazeeat { get; set; }
    }
}