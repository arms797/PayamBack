// Services/Implementations/SaatBargozariCacheService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Edu;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class SaatBargozariCacheService : ISaatBargozariCacheService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private const string AllActiveKey = "SaatBargozari_AllActive";
        private const string ByCodePrefix = "SaatBargozari_Code_";

        public SaatBargozariCacheService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<SaatBargozariKelasha>> GetAllActiveAsync()
        {
            if (_cache.TryGetValue(AllActiveKey, out List<SaatBargozariKelasha>? cached) && cached != null)
                return cached;

            var saats = await _context.SaatBargozariKelashas
                .Where(s => s.Hozoori == true || s.Majazi == true)
                .OrderBy(s => s.CodeSaat)
                .ToListAsync();

            _cache.Set(AllActiveKey, saats, TimeSpan.FromHours(6));
            return saats;
        }

        public async Task<SaatBargozariKelasha?> GetByCodeAsync(string code)
        {
            if (string.IsNullOrEmpty(code))
                return null;

            var cacheKey = $"{ByCodePrefix}{code}";
            if (_cache.TryGetValue(cacheKey, out SaatBargozariKelasha? cached) && cached != null)
                return cached;

            var saat = await _context.SaatBargozariKelashas
                .FirstOrDefaultAsync(s => s.CodeSaat == code);

            if (saat != null)
                _cache.Set(cacheKey, saat, TimeSpan.FromHours(6));

            return saat;
        }

        public async Task<List<SaatBargozariKelasha>> GetByNoeAnjamAsync(int noeAnjam)
        {
            var all = await GetAllActiveAsync();

            return noeAnjam switch
            {
                1 => all.Where(s => s.Hozoori == true).ToList(),      // حضوری
                2 => all.Where(s => s.Majazi == true).ToList(),       // مجازی
                3 => all.Where(s => s.Hozoori == true && s.Majazi == true).ToList(), // ترکیبی
                _ => all
            };
        }

        public void ClearCache()
        {
            _cache.Remove(AllActiveKey);
            // پاک کردن کلیدهای ByCode (با پیشوند)
            // در IMemoryCache نمی‌توان گروهی حذف کرد، پس فقط کلیدهای مشخص را حذف می‌کنیم
            var codes = new[] { "A", "B", "C", "D", "E", "F", "G", "H" };
            foreach (var code in codes)
            {
                _cache.Remove($"{ByCodePrefix}{code}");
            }
        }
    }
}