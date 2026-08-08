using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    public class HamjavarConfirmDto
    {
        [MaxLength(100)]
        public string? Nazar { get; set; }
    }
}