namespace PayamBack.DTOs.Identity.RoleAssignment
{
    public class RoleAssignmentUpdateDto
    {
        public bool? IsDefault { get; set; }
        public int? ParentUserRoleId { get; set; }
    }
}