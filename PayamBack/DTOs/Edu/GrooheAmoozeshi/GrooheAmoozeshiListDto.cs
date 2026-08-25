using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.GrooheAmoozeshi
{
    public class GrooheAmoozeshiListDto
    {
        public int Id { get; set; }
        public string CodeDaneshkade { get; set; } = string.Empty;
        public string NaamDaneshkadeh { get; set; } = string.Empty;
        public string CodeGrooheAmoozeshi { get; set; } = string.Empty;
        public string OnvanGrooheAmoozeshi { get; set; } = string.Empty;
    }
}