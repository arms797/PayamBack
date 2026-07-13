namespace PayamBack.DTOs.Identity.RolePermission
{
    public class RolePermissionByPermissionDto
    {
        public int Id { get; set; }
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public bool Vazeeat { get; set; }
    }
}