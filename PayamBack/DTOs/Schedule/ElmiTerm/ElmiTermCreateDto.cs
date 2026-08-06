using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermCreateDto
    {
        [Required(ErrorMessage = "شناسه استاد الزامی است")]
        public int UserId { get; set; }

        [Required(ErrorMessage = "کد ترم الزامی است")]
        [MaxLength(50)]
        public string TermCode { get; set; } = string.Empty;

        [MaxLength(100)]
        public string? AkharinVazeeat { get; set; }

        public bool? IsEjeari { get; set; }

        [MaxLength(200)]
        public string? OnvanEjraei { get; set; }

        public bool? FullTime { get; set; }

        [MaxLength(50)]
        public string? TedadSaatMovazafi { get; set; }

        public IFormFile? File { get; set; }

        public int? CopyFromId { get; set; }
    }
}