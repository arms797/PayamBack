using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Identity.RoleAssignment
{
    public class RoleAssignmentCreateDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        public int RoleId { get; set; }

        [Required]
        public int MarkazId { get; set; }

        public bool? IsDefault { get; set; }

        public int? ParentUserRoleId { get; set; }
    }
}