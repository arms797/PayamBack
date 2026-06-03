using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models
{
    public class SaatBargozariKelasha
    {
        [Key]
        public int Id { get; set; }

        [Required, MaxLength(100)]
        public string OnvanSaat { get; set; } = "";//ساعت اول , ساعت دوم

        [Required, MaxLength(1)]
        public string CodeSaat { get; set; } = "";//A,B,C,D,E,F,G,H

        [Required, MaxLength(10)]
        public string SaatShoroo { get; set; } = "";   // مثلاً "08:00"

        [Required, MaxLength(10)]
        public string SaatPayan { get; set; } = "";    // مثلاً "10:30"

        [Required]
        public bool Hozoori  { get; set; } = true;     // مثلاً "فعال" یا "غیرفعال"
        [Required]
        public bool Majazi { get; set; } = true;     // مثلاً "فعال" یا "غیرفعال"
    }

}
