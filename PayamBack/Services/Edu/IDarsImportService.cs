using PayamBack.DTOs.Edu.Dars;

namespace PayamBack.Services.Edu
{
    /// <summary>
    /// سرویس ایمپورت گروهی درس‌ها و منابع از فایل اکسل.
    /// 
    /// این سرویس منطق Sync را به صورت مستقل پیاده‌سازی می‌کند تا در آینده
    /// هم از BulkUpload (اکسل) و هم از API (JSON) قابل استفاده باشد.
    /// 
    /// منطق:
    ///   - رشته پیدا نشد → skip + گزارش
    ///   - درس جدید → Insert
    ///   - درس موجود → Update (همه فیلدها + Saat با منطق شرطی)
    ///   - منبع جدید → Insert
    ///   - منبع موجود → Update
    ///   - منبع غایب در فایل → Hard Delete (فقط برای درس‌های حاضر در فایل)
    /// </summary>
    public interface IDarsImportService
    {
        /// <summary>
        /// پردازش گروه‌های رشته و ایمپورت درس‌ها و منابع.
        /// </summary>
        Task<ImportResultDto> ImportAsync(
            List<ReshtehGroup> reshtehGroups,
            CancellationToken cancellationToken = default);
    }
}
