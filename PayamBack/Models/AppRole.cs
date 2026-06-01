using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models
{
    public class AppRole:IdentityRole<int>
    {
        [Required]
        public int CodeGrooheKarbari { get; set; }
        [Required]
        public bool Vazeeyat { get; set; }
        public bool Emza { get; set; } = false;
    }
}
