// در فایل DTOs/Core/Karmand/ToggleUserStatusItemDto.cs
namespace PayamBack.DTOs.Core.Karmand
{
    public class ToggleUserStatusItemDto
    {
        /// <summary>شناسه کاربر</summary>
        public int UserId { get; set; }

        /// <summary>وضعیت فعال/غیرفعال (اختیاری - اگر null باشد، تغییری نمی‌کند)</summary>
        public bool? Vazeeyat { get; set; }

        /// <summary>وضعیت موقت (اختیاری - اگر null باشد، تغییری نمی‌کند)</summary>
        public bool? VazeeyatMovaghat { get; set; }
    }
}