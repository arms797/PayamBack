using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermCreateDto
    {
        [Required(ErrorMessage = "شناسه استاد الزامی است")]
        public int UserId { get; set; }

        public string? AkharinVazeeat { get; set; }

        public bool? IsEjeari { get; set; }

        [MaxLength(200)]
        public string? OnvanEjraei { get; set; }

        public bool? FullTime { get; set; }

        public int? TedadSaatMovazafi { get; set; }

        public decimal? TedadVahedMovazafi { get; set; }

        public IFormFile? File { get; set; }
    }
}