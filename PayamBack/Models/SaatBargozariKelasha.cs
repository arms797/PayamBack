using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models
{
    public class SaatBargozariKelasha
    {
        [Key]
        public int Id { get; set; }

        // عنوان ساعت (ساعت اول، ساعت دوم، ...)
        [Required, MaxLength(100)]
        public string OnvanSaat { get; set; } = "";

        // کد ساعت (A, B, C, D, E, F, G, H)
        [Required, MaxLength(1)]
        public string CodeSaat { get; set; } = "";

        // ساعت شروع (مثلاً "08:00")
        [Required, MaxLength(5)]
        public string SaatShoroo { get; set; } = "";

        // ساعت پایان (مثلاً "10:30")
        [Required, MaxLength(5)]
        public string SaatPayan { get; set; } = "";

        // وضعیت حضور (فعال/غیرفعال)
        [Required]
        public bool Hozoori { get; set; } = true;

        // وضعیت مجازی (فعال/غیرفعال)
        [Required]
        public bool Majazi { get; set; } = true;
    }
}