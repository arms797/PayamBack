// PayamBack/Services/Implementations/MarkazCacheService.cs
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class MarkazCacheService : IMarkazCacheService
    {
        private readonly AppDbContext _context;
        private readonly IMemoryCache _cache;

        private const string AllMarkazListKey = "AllMarkazList";
        private const string AllMarkazDictionaryKey = "AllMarkazDictionary";
        // 🔥 کش جدید (همه مراکز - فعال + غیرفعال)
        private const string AllMarkazIncludingInactiveListKey = "AllMarkazIncludingInactiveList";
        private const string AllMarkazIncludingInactiveDictionaryKey = "AllMarkazIncludingInactiveDictionary";

        public MarkazCacheService(AppDbContext context, IMemoryCache cache)
        {
            _context = context;
            _cache = cache;
        }

        public async Task<List<Markaz>> GetAllAsync()
        {
            if (_cache.TryGetValue(AllMarkazListKey, out List<Markaz>? markazList) && markazList != null)
                return markazList;

            markazList = await _context.Markazes
                .Where(m => m.Vazeeyat == true)
                .OrderBy(m => m.NaamMarkaz)
                .ToListAsync();

            _cache.Set(AllMarkazListKey, markazList, TimeSpan.FromHours(6));
            return markazList;
        }

        public async Task<Markaz?> GetByIdAsync(int id)
        {
            var dictionary = await GetDictionaryAsync();
            return dictionary.TryGetValue(id, out var markaz) ? markaz : null;
        }

        public async Task<string?> GetNameByIdAsync(int id)
        {
            var markaz = await GetByIdAsync(id);
            return markaz?.NaamMarkaz;
        }

        public async Task<Dictionary<int, Markaz>> GetDictionaryAsync()
        {
            if (_cache.TryGetValue(AllMarkazDictionaryKey, out Dictionary<int, Markaz>? dictionary) && dictionary != null)
                return dictionary;

            var list = await GetAllAsync();
            dictionary = list.ToDictionary(m => m.Id);
            _cache.Set(AllMarkazDictionaryKey, dictionary, TimeSpan.FromHours(6));
            return dictionary;
        }

        // ============================================================
        // 🔴 متدهای جدید (همه مراکز - فعال + غیرفعال)
        // ============================================================

        /// <summary>
        /// دریافت همه مراکز (فعال + غیرفعال) از کش
        /// </summary>
        public async Task<List<Markaz>> GetAllIncludingInactiveAsync()
        {
            if (_cache.TryGetValue(AllMarkazIncludingInactiveListKey, out List<Markaz>? markazList) && markazList != null)
                return markazList;

            markazList = await _context.Markazes
                .OrderBy(m => m.NaamMarkaz)
                .ToListAsync();

            _cache.Set(AllMarkazIncludingInactiveListKey, markazList, TimeSpan.FromHours(6));
            return markazList;
        }

        /// <summary>
        /// دریافت یک مرکز با Id (شامل غیرفعال‌ها)
        /// </summary>
        public async Task<Markaz?> GetByIdIncludingInactiveAsync(int id)
        {
            var dictionary = await GetIncludingInactiveDictionaryAsync();
            return dictionary.TryGetValue(id, out var markaz) ? markaz : null;
        }

        /// <summary>
        /// دریافت نام مرکز (شامل غیرفعال‌ها)
        /// </summary>
        public async Task<string?> GetNameByIdIncludingInactiveAsync(int id)
        {
            var markaz = await GetByIdIncludingInactiveAsync(id);
            return markaz?.NaamMarkaz;
        }

        /// <summary>
        /// دریافت Dictionary همه مراکز (فعال + غیرفعال)
        /// </summary>
        public async Task<Dictionary<int, Markaz>> GetIncludingInactiveDictionaryAsync()
        {
            if (_cache.TryGetValue(AllMarkazIncludingInactiveDictionaryKey, out Dictionary<int, Markaz>? dictionary) && dictionary != null)
                return dictionary;

            var list = await GetAllIncludingInactiveAsync();
            dictionary = list.ToDictionary(m => m.Id);
            _cache.Set(AllMarkazIncludingInactiveDictionaryKey, dictionary, TimeSpan.FromHours(6));
            return dictionary;
        }

        // ============================================================
        // 🔥 پاک کردن کش (همه کش‌ها)
        // ============================================================
        public void ClearCache()
        {
            _cache.Remove(AllMarkazListKey);
            _cache.Remove(AllMarkazDictionaryKey);
            _cache.Remove(AllMarkazIncludingInactiveListKey);
            _cache.Remove(AllMarkazIncludingInactiveDictionaryKey);
        }
    }
}