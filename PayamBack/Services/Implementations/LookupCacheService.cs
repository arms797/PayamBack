using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Edu;
using PayamBack.Models.Schedule;
using PayamBack.Services.Interfaces;

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
                    .ToListAsync()
            };

            _cache.Set(CacheKey, data, TimeSpan.FromHours(6));
            return data;
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

        public void ClearCache()
        {
            _cache.Remove(CacheKey);
        }
    }
}