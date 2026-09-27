using PayamBack.Models.Core;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    /// <summary>
    /// جدول واسط بین درس ارائه شده و استادها
    /// هر رکورد = یک استاد برای یک درس ارائه شده
    /// </summary>
    [Table("DarsEraehOstad")]
    public class DarsEraehOstad
    {
        // ============================================================
        // کلید اصلی
        // ============================================================
        [Key]
        [DatabaseGenerated(DatabaseGeneratedOption.Identity)]
        public int Id { get; set; }

        // ============================================================
        // روابط
        // ============================================================
        /// <summary>شناسه استاد (FK به Ostad)</summary>
        [Required(ErrorMessage = "استاد الزامی است")]
        public int OstadId { get; set; }

        [ForeignKey(nameof(OstadId))]
        public virtual Ostad? Ostad { get; set; }

        /// <summary>شناسه درس ارائه شده (FK به DarsEraeh)</summary>
        [Required(ErrorMessage = "درس ارائه شده الزامی است")]
        public int DarsEraehId { get; set; }

        [ForeignKey(nameof(DarsEraehId))]
        public virtual DarsEraeh? DarsEraeh { get; set; }

        // ============================================================
        // نقش استاد
        // ============================================================
        /// <summary>آیا استاد اصلی است؟</summary>
        public bool Asli { get; set; } = false;
    }
}