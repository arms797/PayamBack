using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    /// <summary>
    /// جدول منابع درسی
    /// هر درس معمولاً یک منبع داره، ولی بعضی دروس ۲ یا ۳ منبع دارن
    /// </summary>
    [Table("ManbaDars")]
    public class ManbaDars
    {
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        [Required]
        public int DarsId { get; set; }

        [ForeignKey(nameof(DarsId))]
        public virtual Dars? Dars { get; set; }

        [StringLength(20)]
        public string? ShomareManba { get; set; }

        [StringLength(100)]
        public string? NoeManba { get; set; }

        [Required]
        [StringLength(300)]
        public string Onvan { get; set; } = string.Empty;

        [StringLength(200)]
        public string? Nevisandeh { get; set; }

        [StringLength(200)]
        public string? Motarjem { get; set; }

        [StringLength(10)]
        public string? SalEnteshar { get; set; }

        [StringLength(10)]
        public string? SalEntesharMiladi { get; set; }

        [StringLength(30)]
        public string? Shabak { get; set; }

        [StringLength(200)]
        public string? Nasher { get; set; }

        [StringLength(20)]
        public string? NobateChap { get; set; }

        [StringLength(100)]
        public string? Vazeeyat { get; set; }

        [StringLength(50)]
        public string? CodePeyvast { get; set; }

        [StringLength(1000)]
        public string? SharhPeyvast { get; set; }
        [StringLength(20)]
        public string? TermUpdate {  get; set; }

        public DateTime? CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }
}