using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.Dars
{
    public class ManbaDarsCreateDto
    {
        [StringLength(20)]
        public string? ShomareManba { get; set; }

        [StringLength(100)]
        public string? NoeManba { get; set; }

        [Required(ErrorMessage = "عنوان منبع الزامی است")]
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
    }
}