using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.OstadDars
{
    public class OstadDarsCreateDto
    {
        [Required(ErrorMessage = "استاد الزامی است")]
        public int OstadId { get; set; }

        [Required(ErrorMessage = "درس ارائه شده الزامی است")]
        public int DarsEraehId { get; set; }

        public bool Asli { get; set; } = false;
    }
}