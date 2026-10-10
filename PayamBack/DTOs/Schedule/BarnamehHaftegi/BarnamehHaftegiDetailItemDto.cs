namespace PayamBack.DTOs.Schedule.BarnamehHaftegi
{
    public class BarnamehHaftegiDetailItemDto
    {
        public int Id { get; set; }
        public string RoozeHafteh { get; set; } = string.Empty;
        public string RoozeHaftehDisplay { get; set; } = string.Empty;

        // 🔥 فیلدهای جدید برای مرکز اصلی روز
        public int? MarkazId { get; set; }
        public string? MarkazName { get; set; }

        public int? A { get; set; }
        public int? MarkazIdA { get; set; }
        public string? MarkazNameA { get; set; }

        public int? B { get; set; }
        public int? MarkazIdB { get; set; }
        public string? MarkazNameB { get; set; }

        public int? C { get; set; }
        public int? MarkazIdC { get; set; }
        public string? MarkazNameC { get; set; }

        public int? D { get; set; }
        public int? MarkazIdD { get; set; }
        public string? MarkazNameD { get; set; }

        public int? E { get; set; }
        public int? MarkazIdE { get; set; }
        public string? MarkazNameE { get; set; }

        public int? F { get; set; }
        public int? MarkazIdF { get; set; }
        public string? MarkazNameF { get; set; }

        public int? G { get; set; }
        public int? MarkazIdG { get; set; }
        public string? MarkazNameG { get; set; }

        public int? H { get; set; }
        public int? MarkazIdH { get; set; }
        public string? MarkazNameH { get; set; }

        public bool? Jozeiat { get; set; }
        public bool IsPermittedDay { get; set; }
    }
}
