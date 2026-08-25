using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.Reshteh
{
    public class ReshtehUpdateDto
    {
        public int? GrooheAmoozeshiId { get; set; }

        [MaxLength(50)]
        public string? CodeMaghta { get; set; }

        [MaxLength(100)]
        public string? Maghta { get; set; }

        [MaxLength(50)]
        public string? CodeReshte { get; set; }

        [MaxLength(200)]
        public string? OnvanReshte { get; set; }

        [MaxLength(10)]
        public string? TermVorood { get; set; }

        [MaxLength(10)]
        public string? TermEamal { get; set; }
    }
}