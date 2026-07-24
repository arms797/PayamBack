namespace PayamBack.DTOs.Identity.RoleAssignment
{
    public class RoleAssignmentDetailDto
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int RoleId { get; set; }
        public string RoleName { get; set; } = string.Empty;
        public int CodeRole { get; set; }
        public bool IsAdmin { get; set; }
        public bool IsUniquePerMarkaz { get; set; }
        public int MarkazId { get; set; }
        public string MarkazName { get; set; } = string.Empty;
        public int MarkazLevel { get; set; }
        public bool IsDefault { get; set; }
        public int? ParentUserRoleId { get; set; }
        public string ParentUserName { get; set; } = string.Empty;
    }
}