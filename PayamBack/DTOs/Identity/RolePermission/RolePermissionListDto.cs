namespace PayamBack.DTOs.Identity.RolePermission
{
    public class RolePermissionListDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int PermissionId { get; set; }
        public string PermissionName { get; set; } = string.Empty;
        public bool Vazeeat { get; set; }
    }
}