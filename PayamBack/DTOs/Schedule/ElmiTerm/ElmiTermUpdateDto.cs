using Microsoft.AspNetCore.Http;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.ElmiTerm
{
    public class ElmiTermUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(100)]
        public string? AkharinVazeeat { get; set; }

        public bool? IsEjeari { get; set; }

        [MaxLength(200)]
        public string? OnvanEjraei { get; set; }

        public bool? FullTime { get; set; }

        [MaxLength(50)]
        public string? TedadSaatMovazafi { get; set; }

        public IFormFile? File { get; set; }
    }
}