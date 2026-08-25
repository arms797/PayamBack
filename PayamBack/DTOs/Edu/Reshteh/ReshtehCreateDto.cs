using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.Reshteh
{
    public class ReshtehCreateDto
    {
        [Required(ErrorMessage = "گروه آموزشی الزامی است")]
        public int GrooheAmoozeshiId { get; set; }

        [MaxLength(50)]
        public string? CodeMaghta { get; set; }

        [MaxLength(100)]
        public string? Maghta { get; set; }

        [MaxLength(50)]
        public string? CodeReshte { get; set; }

        [Required(ErrorMessage = "عنوان رشته الزامی است")]
        [MaxLength(200)]
        public string OnvanReshte { get; set; } = string.Empty;

        [MaxLength(10)]
        public string? TermVorood { get; set; }

        [MaxLength(10)]
        public string? TermEamal { get; set; }
    }
}