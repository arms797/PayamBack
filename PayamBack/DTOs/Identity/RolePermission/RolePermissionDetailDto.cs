namespace PayamBack.DTOs.Identity.RolePermission
{
    public class RolePermissionDetailDto
    {
        public int Id { get; set; }
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public string Resource { get; set; } = string.Empty;
        public string Action { get; set; } = string.Empty;
        public bool Vazeeat { get; set; }
    }
}