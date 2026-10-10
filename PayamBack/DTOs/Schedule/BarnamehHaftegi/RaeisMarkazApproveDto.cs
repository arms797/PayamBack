using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class RaeisMarkazApproveDto
    {
        [Required]
        [Range(1, 2, ErrorMessage = "مقدار باید ۱ (تایید) یا ۲ (رد) باشد")]
        public int ApproveStatus { get; set; }
    }
}
