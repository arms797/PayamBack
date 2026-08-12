using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Schedule.Hamjavar
{
    /// <summary>
    /// DTO برای تایید نهایی توسط استاد
    /// </summary>
    public class HamjavarConfirmDto
    {
        /// <summary>
        /// نظر استاد (عددی)
        /// 1=پیش‌نویس استاد, 2=تایید, 3=رد, 4=اصلاح
        /// </summary>
        [Required]
        public int Nazar { get; set; }
    }
}