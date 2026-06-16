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
        [Required]
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; } = "";

        /// <summary>نام دانشکده</summary>
        [Required]
        [MaxLength(200)]
        public string NaamDaneshkadeh { get; set; } = "";

        /// <summary>کد گروه آموزشی</summary>
        [Required]
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; } = "";

        /// <summary>عنوان گروه آموزشی</summary>
        [Required]
        [MaxLength(200)]
        public string OnvanGrooheAmoozeshi { get; set; } = "";

        /// <summary>کد ترکیبی دانشکده/گروه آموزشی</summary>
        [Required]
        [MaxLength(50)]
        public string CodeTarkibi { get; set; } = "";

        public ICollection<Reshteh>? Reshtehs { get; set; }
    }
}
