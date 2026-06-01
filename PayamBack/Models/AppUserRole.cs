using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models
{
    public class AppUserRole:IdentityUserRole<int>
    {
        [Required]
        [MaxLength(50)]
        public string CodeOstan { get; set; } = "";
        [Required]
        [MaxLength(50)]
        public string CodeMarkaz { get; set; } = "";
        [Required]
        public bool RolePishFarz { get; set; }=false;
    }
}
