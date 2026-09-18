using System.ComponentModel.DataAnnotations;

namespace PayamBack.DTOs.Ostad
{
    /// <summary>
    /// DTO اختصاصی برای تغییر مرکز خدمتی و مرکز اصلی استاد
    /// </summary>
    public class ChangeOstadMarkazDto
    {
        /// <summary>
        /// شناسه مرکز خدمتی (اجباری)
        /// </summary>
        //[Required(ErrorMessage = "انتخاب مرکز خدمتی الزامی است")]
        public int? MarkazId { get; set; }

        /// <summary>
        /// شناسه مرکز اصلی (اختیاری)
        /// اگر خالی باشد، مرکز خدمتی به عنوان مرکز اصلی در نظر گرفته می‌شود
        /// </summary>
        public int? MarkazAsliId { get; set; }
    }
}