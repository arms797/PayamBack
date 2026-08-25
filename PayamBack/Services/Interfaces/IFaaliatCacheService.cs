// Services/Interfaces/IFaaliatCacheService.cs
using PayamBack.Models.Schedule;

namespace PayamBack.Services.Interfaces
{
    public interface IFaaliatCacheService
    {
        /// <summary>دریافت لیست تمام فعالیت‌های فعال</summary>
        Task<List<Faaliat>> GetAllActiveAsync();

        /// <summary>دریافت یک فعالیت با شناسه</summary>
        Task<Faaliat?> GetByIdAsync(int id);

        /// <summary>دریافت فعالیت‌ها بر اساس نوع انجام (حضوری، مجازی، ترکیبی)</summary>
        Task<List<Faaliat>> GetByNoeAnjamAsync(int noeAnjam);

        /// <summary>دریافت فعالیت‌های مجاز برای مدعو</summary>
        Task<List<Faaliat>> GetMadoveAllowedAsync();

        /// <summary>پاک کردن کش</summary>
        void ClearCache();
    }
}