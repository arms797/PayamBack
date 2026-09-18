using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Ostad
{
    /// <summary>
    /// DTO برای تغییر مراکز استاد بر اساس کد استادی (فقط ادمین سامانه)
    /// </summary>
    public class ChangeMarkazByCodeDto
    {
        [Required(ErrorMessage = "کد استادی الزامی است")]
        public string CodeOstadi { get; set; } = string.Empty;

        [Required(ErrorMessage = "مرکز خدمتی الزامی است")]
        public int MarkazId { get; set; }

        public int? MarkazAsliId { get; set; }
    }
}