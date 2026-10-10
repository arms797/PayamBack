using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BarnamehHaftegiDetailCreateDto
    {
        [Required]
        [MaxLength(10)]
        public string RoozeHafteh { get; set; } = string.Empty;

        [Required]
        public int MarkazId { get; set; }  // 🔥 مرکز اصلی روز

        public int? A { get; set; }
        public int? MarkazIdA { get; set; }
        public int? B { get; set; }
        public int? MarkazIdB { get; set; }
        public int? C { get; set; }
        public int? MarkazIdC { get; set; }
        public int? D { get; set; }
        public int? MarkazIdD { get; set; }
        public int? E { get; set; }
        public int? MarkazIdE { get; set; }
        public int? F { get; set; }
        public int? MarkazIdF { get; set; }
        public int? G { get; set; }
        public int? MarkazIdG { get; set; }
        public int? H { get; set; }
        public int? MarkazIdH { get; set; }
        public bool? Jozeiat { get; set; }
    }
}
