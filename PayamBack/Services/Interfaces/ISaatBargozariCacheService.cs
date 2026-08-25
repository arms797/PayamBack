// Services/Interfaces/ISaatBargozariCacheService.cs
using PayamBack.Models.Edu;

namespace PayamBack.Services.Interfaces
{
    public interface ISaatBargozariCacheService
    {
        /// <summary>دریافت لیست تمام ساعت‌های فعال (Hozoori=true یا Majazi=true)</summary>
        Task<List<SaatBargozariKelasha>> GetAllActiveAsync();

        /// <summary>دریافت یک ساعت با کد (A, B, C, ...)</summary>
        Task<SaatBargozariKelasha?> GetByCodeAsync(string code);

        /// <summary>دریافت لیست ساعت‌های مجاز برای نوع فعالیت (حضوری/مجازی)</summary>
        Task<List<SaatBargozariKelasha>> GetByNoeAnjamAsync(int noeAnjam);

        /// <summary>پاک کردن کش</summary>
        void ClearCache();
    }
}