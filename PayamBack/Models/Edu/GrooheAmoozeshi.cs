using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// گروه آموزشی
    /// </summary>
    public class GrooheAmoozeshi
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        /// <summary>کد دانشکده</summary>
        [MaxLength(50)]
        public string? CodeDaneshkade { get; set; }

        /// <summary>نام دانشکده</summary>
        [MaxLength(200)]
        public string? NaamDaneshkadeh { get; set; }

        /// <summary>کد گروه آموزشی</summary>
        [MaxLength(50)]
        public string? CodeGrooheAmoozeshi { get; set; }

        /// <summary>عنوان گروه آموزشی</summary>
        [MaxLength(200)]
        public string? OnvanGrooheAmoozeshi { get; set; }

        /// <summary>کد ترکیبی دانشکده/گروه آموزشی</summary>
        [MaxLength(50)]
        public string? CodeTarkibi { get; set; }

        /// <summary>رشته‌های تحصیلی مرتبط با این گروه آموزشی</summary>
        public virtual ICollection<Reshteh>? Reshtehs { get; set; }
    }
}