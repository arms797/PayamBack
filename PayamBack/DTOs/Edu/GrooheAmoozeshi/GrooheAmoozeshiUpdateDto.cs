using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.GrooheAmoozeshi
{
    public class GrooheAmoozeshiUpdateDto
    {
        [MaxLength(50)]
        public string? CodeDaneshkade { get; set; }

        [MaxLength(200)]
        public string? NaamDaneshkadeh { get; set; }

        [MaxLength(50)]
        public string? CodeGrooheAmoozeshi { get; set; }

        [MaxLength(200)]
        public string? OnvanGrooheAmoozeshi { get; set; }
    }
}