// Models/Schedule/FaaliatGroup.cs
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Models.Schedule
{
    public class FaaliatGroup
    {
        [Key]
        public int Id { get; set; }

        [MaxLength(100)]
        public string Title { get; set; } = string.Empty;  // مثلاً "تدریس"

        [MaxLength(200)]
        public string? Description { get; set; }

        public bool IsActive { get; set; } = true;

        // ============================================================
        // محدودیت‌های گروهی (اختیاری)
        // ============================================================
        public int? MinSaatDarHafteh { get; set; }
        public int? MaxSaatDarHafteh { get; set; }
        public int? MinDayDarHafteh { get; set; }
        public int? MaxDayDarHafteh { get; set; }

        // Navigation
        public virtual ICollection<Faaliat>? Faaliats { get; set; }
    }
}