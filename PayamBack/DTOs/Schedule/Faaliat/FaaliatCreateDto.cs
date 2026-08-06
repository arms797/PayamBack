using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Faaliat
{
    public class FaaliatCreateDto
    {
        [Required(ErrorMessage = "عنوان فعالیت الزامی است")]
        [MaxLength(200)]
        public string Onvan { get; set; } = string.Empty;

        [Required(ErrorMessage = "نحوه انجام الزامی است")]
        public int NoeAnjam { get; set; }

        public int? MinSaatDarEdari { get; set; }
        public int? MaxSaatDarEdari { get; set; }
        public int? MinSaatDarHafteh { get; set; }
        public int? MaxSaatDarHafteh { get; set; }
        public int? MinDayDarHafteh { get; set; }
        public int? MaxDayDarHafteh { get; set; }
        public bool? IsMadove { get; set; }

        [MaxLength(20)]
        public string? Color { get; set; }

        public bool? Vazeeat { get; set; }
    }
}