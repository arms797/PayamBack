// Models/Edu/WeekDay.cs
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Edu
{
    public class WeekDay
    {
        [Key]
        public int Id { get; set; }

        /// <summary>کد عددی روز (1=شنبه، 2=یکشنبه، ...، 7=جمعه)</summary>
        public int Code { get; set; }

        /// <summary>عنوان روز (شنبه، یکشنبه، ...)</summary>
        [MaxLength(20)]
        public string Title { get; set; } = string.Empty;

        /// <summary>آیا این روز برای برنامه‌ریزی فعال است؟ (true=قابل انتخاب)</summary>
        public bool IsActive { get; set; } = true;

        /// <summary>ترتیب نمایش (برای مرتب‌سازی)</summary>
        public int Order { get; set; }

        /// <summary>آیا این روز تعطیل رسمی است؟ (فقط برای نمایش/راهنما)</summary>
        public bool IsHoliday { get; set; } = false;
    }
}