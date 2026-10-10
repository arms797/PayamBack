using PayamBack.Controllers.Schedule;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BarnamehHaftegiCreateDto
    {
        [Required]
        public int OstadId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CodeTerm { get; set; } = string.Empty;

        [Required]
        public List<BarnamehHaftegiDetailCreateDto> Details { get; set; } = new();
    }
}
