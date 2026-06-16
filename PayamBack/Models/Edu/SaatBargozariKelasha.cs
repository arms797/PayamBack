using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class SaatBargozariKelasha
    {
        [Key]
        public int Id { get; set; }

        /// <summary>عنوان ساعت (ساعت اول، ساعت دوم، ...)</summary>
        [MaxLength(100)]
        public string? OnvanSaat { get; set; }

        /// <summary>کد ساعت (A, B, C, D, E, F, G, H)</summary>
        [MaxLength(1)]
        public string? CodeSaat { get; set; }

        /// <summary>ساعت شروع (مثلاً "08:00")</summary>
        [MaxLength(5)]
        public string? SaatShoroo { get; set; }

        /// <summary>ساعت پایان (مثلاً "10:30")</summary>
        [MaxLength(5)]
        public string? SaatPayan { get; set; }

        /// <summary>وضعیت حضور (فعال/غیرفعال)</summary>
        public bool? Hozoori { get; set; }

        /// <summary>وضعیت مجازی (فعال/غیرفعال)</summary>
        public bool? Majazi { get; set; }
    }
}