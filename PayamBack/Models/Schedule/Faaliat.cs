using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PayamBack.Models.Schedule
{
    /// <summary>
    /// مدل فعالیت‌های قابل انتخاب در درخواست هم‌جاوری
    /// </summary>
    public class Faaliat
    {
        [Key]
        public int Id { get; set; }

        // ============================================================
        // اطلاعات پایه فعالیت
        // ============================================================

        /// <summary>عنوان فعالیت</summary>
        [MaxLength(200)]
        public string? Onvan { get; set; }

        /// <summary>
        /// نحوه انجام فعالیت
        /// 1=حضوری
        /// 2=مجازی
        /// 3=ترکیبی
        /// </summary>
        public int? NoeAnjam { get; set; }

        // ============================================================
        // محدودیت‌های زمانی (ساعتی)
        // ============================================================

        /// <summary>حداقل ساعت در تایم اداری</summary>
        public int? MinSaatDarEdari { get; set; }

        /// <summary>حداکثر ساعت در تایم اداری</summary>
        public int? MaxSaatDarEdari { get; set; }

        // ============================================================
        // محدودیت‌های زمانی (هفتگی)
        // ============================================================

        /// <summary>حداقل ساعت در هفته</summary>
        public int? MinSaatDarHafteh { get; set; }

        /// <summary>حداکثر ساعت در هفته</summary>
        public int? MaxSaatDarHafteh { get; set; }

        // ============================================================
        // محدودیت‌های روزانه
        // ============================================================

        /// <summary>حداقل روز در هفته</summary>
        public int? MinDayDarHafteh { get; set; }

        /// <summary>حداکثر روز در هفته</summary>
        public int? MaxDayDarHafteh { get; set; }

        // ============================================================
        // سایر ویژگی‌ها
        // ============================================================

        /// <summary>اعمال برای مدعو (true = بله)</summary>
        public bool? IsMadove { get; set; }

        /// <summary>رنگ فعالیت (برای نمایش در تقویم)</summary>
        [MaxLength(20)]
        public string? Color { get; set; }

        /// <summary>وضعیت (فعال/غیرفعال)</summary>
        public bool? Vazeeat { get; set; }

        public int? FaaliatGroupId { get; set; }

        public virtual FaaliatGroup? FaaliatGroup { get; set; }
    }
}