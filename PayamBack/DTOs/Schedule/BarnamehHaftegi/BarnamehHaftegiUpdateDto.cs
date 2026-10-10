using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BarnamehHaftegiUpdateDto
    {
        [Required]
        public List<BarnamehHaftegiDetailCreateDto> Details { get; set; } = new();
    }
}
