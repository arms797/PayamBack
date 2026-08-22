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

        public void ClearCache()
        {
            _cache.Remove(AllMarkazListKey);
            _cache.Remove(AllMarkazDictionaryKey);
        }
    }
}