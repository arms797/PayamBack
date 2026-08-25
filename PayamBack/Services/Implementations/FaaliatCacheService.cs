// Services/Implementations/FaaliatCacheService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Schedule;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class FaaliatCacheService : IFaaliatCacheService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private const string AllActiveKey = "Faaliats_AllActive";
        private const string ByNoeAnjamPrefix = "Faaliats_NoeAnjam_";
        private const string MadoveAllowedKey = "Faaliats_MadoveAllowed";

        public FaaliatCacheService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Faaliat>> GetAllActiveAsync()
        {
            if (_cache.TryGetValue(AllActiveKey, out List<Faaliat>? cached) && cached != null)
                return cached;

            var faaliats = await _context.Faaliats
                .Where(f => f.Vazeeat == true)
                //.OrderBy(f => f.Onvan)
                .ToListAsync();

            _cache.Set(AllActiveKey, faaliats, TimeSpan.FromHours(6));
            return faaliats;
        }

        public async Task<Faaliat?> GetByIdAsync(int id)
        {
            var all = await GetAllActiveAsync();
            return all.FirstOrDefault(f => f.Id == id);
        }

        public async Task<List<Faaliat>> GetByNoeAnjamAsync(int noeAnjam)
        {
            var cacheKey = $"{ByNoeAnjamPrefix}{noeAnjam}";
            if (_cache.TryGetValue(cacheKey, out List<Faaliat>? cached) && cached != null)
                return cached;

            var all = await GetAllActiveAsync();
            var filtered = all
                .Where(f => f.NoeAnjam == noeAnjam || f.NoeAnjam == 3) // ترکیبی هم شامل می‌شود
                .ToList();

            _cache.Set(cacheKey, filtered, TimeSpan.FromHours(6));
            return filtered;
        }

        public async Task<List<Faaliat>> GetMadoveAllowedAsync()
        {
            if (_cache.TryGetValue(MadoveAllowedKey, out List<Faaliat>? cached) && cached != null)
                return cached;

            var all = await GetAllActiveAsync();
            var filtered = all
                .Where(f => f.IsMadove == true)
                .ToList();

            _cache.Set(MadoveAllowedKey, filtered, TimeSpan.FromHours(6));
            return filtered;
        }

        public void ClearCache()
        {
            _cache.Remove(AllActiveKey);
            _cache.Remove(MadoveAllowedKey);
            // پاک کردن همه کلیدهای ByNoeAnjam_*
            // در IMemoryCache نمی‌توان گروهی حذف کرد، اما می‌توانیم کلیدهای مشخص را حذف کنیم
            for (int i = 1; i <= 3; i++)
            {
                _cache.Remove($"{ByNoeAnjamPrefix}{i}");
            }
        }
    }
}