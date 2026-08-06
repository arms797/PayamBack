using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermCopyDto
    {
        [Required]
        public int UserId { get; set; }

        [Required]
        [MaxLength(50)]
        public string SourceTermCode { get; set; } = string.Empty;

        [Required]
        [MaxLength(50)]
        public string TargetTermCode { get; set; } = string.Empty;
    }
}