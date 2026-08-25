using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.GrooheAmoozeshi
{
    public class GrooheAmoozeshiCreateDto
    {
        [Required(ErrorMessage = "کد دانشکده الزامی است")]
        [MaxLength(50)]
        public string CodeDaneshkade { get; set; } = string.Empty;

        [Required(ErrorMessage = "نام دانشکده الزامی است")]
        [MaxLength(200)]
        public string NaamDaneshkadeh { get; set; } = string.Empty;

        [Required(ErrorMessage = "کد گروه آموزشی الزامی است")]
        [MaxLength(50)]
        public string CodeGrooheAmoozeshi { get; set; } = string.Empty;

        [Required(ErrorMessage = "عنوان گروه آموزشی الزامی است")]
        [MaxLength(200)]
        public string OnvanGrooheAmoozeshi { get; set; } = string.Empty;
    }
}