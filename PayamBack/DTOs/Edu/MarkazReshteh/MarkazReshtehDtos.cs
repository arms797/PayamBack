using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Edu.MarkazReshteh
{
    // ============================================================
    // DTO لیست (سبک — رشته از بک‌اند، بقیه از Context)
    // ============================================================
    public class MarkazReshtehListDto
    {
        public int Id { get; set; }

        // شناسه‌های اصلی
        public int MarkazId { get; set; }
        public int ReshtehId { get; set; }

        // 🔥 رشته: کامل از بک‌اند (چون Context ممکنه قدیمی باشه)
        public string? ReshtehName { get; set; }
        public string? CodeReshte { get; set; }
        public string? CodeMaghta { get; set; }
        public string? Maghta { get; set; }

        // 🔥 شناسه‌های کمکی برای Context
        public int? GrooheAmoozeshiId { get; set; }
        public string? CodeDaneshkadeh { get; set; }
        public string? CodeOstan { get; set; }

        // داده‌ی خود رکورد
        public int? TedadDaneshjoo { get; set; }
        public bool Vazeeat { get; set; }
    }

    // ============================================================
    // DTO جزئیات (کامل از بک‌اند)
    // ============================================================
    public class MarkazReshtehDetailDto
    {
        public int Id { get; set; }

        // مرکز (کامل)
        public int MarkazId { get; set; }
        public string? MarkazName { get; set; }
        public string? MarkazCode { get; set; }
        public string? CodeOstan { get; set; }
        public string? OstanName { get; set; }

        // رشته (کامل)
        public int ReshtehId { get; set; }
        public string? ReshtehName { get; set; }
        public string? CodeReshte { get; set; }
        public string? CodeMaghta { get; set; }
        public string? Maghta { get; set; }

        // گروه (کامل)
        public int? GrooheAmoozeshiId { get; set; }
        public string? GrooheAmoozeshiName { get; set; }

        // دانشکده (کامل)
        public string? CodeDaneshkadeh { get; set; }
        public string? DaneshkadehName { get; set; }

        // داده‌ی خود رکورد
        public int? TedadDaneshjoo { get; set; }
        public bool Vazeeat { get; set; }
    }

    // ============================================================
    // DTO ایجاد
    // ============================================================
    public class MarkazReshtehCreateDto
    {
        [Required(ErrorMessage = "مرکز الزامی است")]
        public int MarkazId { get; set; }

        [Required(ErrorMessage = "رشته الزامی است")]
        public int ReshtehId { get; set; }

        [Range(0, int.MaxValue, ErrorMessage = "تعداد دانشجو نمی‌تواند منفی باشد")]
        public int? TedadDaneshjoo { get; set; }

        public bool Vazeeat { get; set; } = true;
    }

    // ============================================================
    // DTO ویرایش
    // ============================================================
    public class MarkazReshtehUpdateDto
    {
        [Range(0, int.MaxValue, ErrorMessage = "تعداد دانشجو نمی‌تواند منفی باشد")]
        public int? TedadDaneshjoo { get; set; }

        public bool? Vazeeat { get; set; }
    }
}
