using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using PayamBack.Models.Identity;
using PayamBack.Models.Edu;

namespace PayamBack.Models.Identity
{
    /// <summary>
    /// رابطه‌ی بین انتصاب نقش (AppUserRole) و گروه آموزشی
    /// هر مدیر گروه برای یک نقش خاص (در یک مرکز/استان) به یک یا چند گروه آموزشی دسترسی دارد
    /// </summary>
    [Table("ModirGrooh")]
    public class ModirGrooh
    {
        [Key]
        public int Id { get; set; }

        public int AppUserRoleId { get; set; }
        public int GrooheAmoozeshiId { get; set; }

        public bool Vazeeat { get; set; } = true;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [ForeignKey(nameof(AppUserRoleId))]
        public virtual AppUserRole? AppUserRole { get; set; }

        [ForeignKey(nameof(GrooheAmoozeshiId))]
        public virtual GrooheAmoozeshi? GrooheAmoozeshi { get; set; }
    }
}