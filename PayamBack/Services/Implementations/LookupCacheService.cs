using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Edu;
using PayamBack.Models.Schedule;
using PayamBack.Services.Interfaces;
using PayamBack.DTOs.Lookup;

namespace PayamBack.Services.Implementations
{
    public class LookupCacheService : ILookupCacheService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;
        private const string CacheKey = "LookupData";

        public LookupCacheService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<LookupData> GetAllAsync()
        {
            if (_cache.TryGetValue(CacheKey, out LookupData? cached) && cached != null)
                return cached;
            var exceptions = await _context.HaftegiExceptions
                .Where(e => _context.Terms.Any(t => t.CodeTerm == e.TermCode && t.Vazeeyat == true) && e.IsActive)
                .ToListAsync();

            var data = new LookupData
            {
                Days = await _context.WeekDays
                    .Where(w => w.IsActive)
                    .OrderBy(w => w.Order)
                    .ToListAsync(),

                Hours = await _context.SaatBargozariKelashas
                    .Where(h => h.Hozoori == true || h.Majazi == true)
                    .OrderBy(h => h.CodeSaat)
                    .ToListAsync(),

                Faaliats = await _context.Faaliats
                    .Where(f => f.Vazeeat == true)
                    .OrderBy(f => f.Id)
                    .ToListAsync(),
                // در LookupCacheService.GetAllAsync
                FaaliatGroups = await _context.FaaliatGroups
                    .Where(g => g.IsActive)
                    .OrderBy(g => g.Title)
                    .ToListAsync(),

                // ✅ استثناهای مربوط به ترم‌های فعال (بدون نیاز به activeTerm جداگانه)
                HaftegiExceptions = exceptions.Select(e => new HaftegiExceptionDto
                {
                    Id = e.Id,
                    TermCode = e.TermCode,
                    OstanCode = e.OstanCode,
                    DayCode = e.DayCode,
                    HourCode = e.HourCode,
                    NoeHamkariMask = e.NoeHamkariMask,
                    FaaliatIds = ConvertFaaliatIdsToList(e.FaaliatIds), // ← تابع کمکی
                    Description = e.Description,
                    IsActive = e.IsActive
                }).ToList()
            };

            _cache.Set(CacheKey, data, TimeSpan.FromHours(6));
            return data;
        }
        public async Task<List<HaftegiExceptionDto>> GetActiveExceptionsAsync()
        {
            var all = await GetAllAsync();
            return all.HaftegiExceptions;
        }

        public async Task<List<WeekDay>> GetActiveDaysAsync()
        {
            var all = await GetAllAsync();
            return all.Days;
        }

        public async Task<List<SaatBargozariKelasha>> GetActiveHoursAsync()
        {
            var all = await GetAllAsync();
            return all.Hours;
        }

        public async Task<List<Faaliat>> GetActiveFaaliatAsync()
        {
            var all = await GetAllAsync();
            return all.Faaliats;
        }

        public async Task<List<FaaliatGroup>> GetActiveFaaliatGroupAsync()
        {
            var all = await GetAllAsync();
            return all.FaaliatGroups;
        }

        public void ClearCache()
        {
            _cache.Remove(CacheKey);
        }
        // 🔧 تابع کمکی برای تبدیل رشته به لیست اعداد
        private List<int>? ConvertFaaliatIdsToList(string? faaliatIds)
        {
            if (string.IsNullOrEmpty(faaliatIds))
                return null; // یعنی همه فعالیت‌ها ممنوع هستند

            return faaliatIds
                .Split('|', StringSplitOptions.RemoveEmptyEntries)
                .Select(int.Parse)
                .ToList();
        }
    }
}