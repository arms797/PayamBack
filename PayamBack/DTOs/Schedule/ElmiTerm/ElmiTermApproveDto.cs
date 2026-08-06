using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermApproveDto
    {
        [Required]
        public int Id { get; set; }

        /// <summary>
        /// 1 = تایید | 2 = رد
        /// </summary>
        [Required]
        [Range(1, 2, ErrorMessage = "مقدار باید 1 (تایید) یا 2 (رد) باشد")]
        public int ApproveStatus { get; set; }

        [MaxLength(500)]
        public string? Tozihat { get; set; }
    }
}