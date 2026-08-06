using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Faaliat
{
    public class FaaliatUpdateDto
    {
        [Required]
        public int Id { get; set; }

        [MaxLength(200)]
        public string? Onvan { get; set; }

        public int? NoeAnjam { get; set; }
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