using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models
{
    public class AppRole:IdentityRole<int>
    {
        [Required]
        public int CodeGrooheKarbari { get; set; }// کد سیستمی یکتا برای نقش
        [Required]
        public bool Vazeeyat { get; set; }// فعال/غیرفعال بودن نقش
        public bool Emza { get; set; } = false;// آیا این نقش نیاز به امضا دارد؟
    }
}
