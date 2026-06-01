using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models
{
    public class AppUser:IdentityUser<int>
    {
        [Required]
        public int CodeNoeUser { get; set; }
        [Required]
        public bool LoginMibashad { get; set; }
        [Required]
        public bool BarayeMogheeLogin { get; set; }
        [Required]
        public bool Vazeeyat { get; set; }
        [Required]
        public bool VazeeyatMovaghat { get; set; }
    }
}
