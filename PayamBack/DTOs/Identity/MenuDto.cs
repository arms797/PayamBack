namespace PayamBack.DTOs.Identity
{
    /// <summary>
    /// DTO منوها به صورت ساختار درختی برای فرانت‌اند
    /// </summary>
    public class MenuDto
    {
        public int Id { get; set; }
        public int? ParentId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Icon { get; set; }
        public string? Path { get; set; }
        public string? PermissionName { get; set; }
        public int? Order { get; set; }
        public List<MenuDto> Children { get; set; } = new();  // زیرمنوها
    }
}