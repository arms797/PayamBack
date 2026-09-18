// PayamBack/Services/Interfaces/IMarkazCacheService.cs
using PayamBack.Models.Core;

namespace PayamBack.Services.Interfaces
{
    public interface IMarkazCacheService
    {
        Task<List<Markaz>> GetAllAsync();
        Task<Markaz?> GetByIdAsync(int id);
        Task<string?> GetNameByIdAsync(int id);
        Task<Dictionary<int, Markaz>> GetDictionaryAsync();

        // 🔴 متدهای جدید (همه مراکز)
        Task<List<Markaz>> GetAllIncludingInactiveAsync();
        Task<Markaz?> GetByIdIncludingInactiveAsync(int id);
        Task<string?> GetNameByIdIncludingInactiveAsync(int id);
        Task<Dictionary<int, Markaz>> GetIncludingInactiveDictionaryAsync();

        void ClearCache();
    }
}