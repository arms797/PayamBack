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
        void ClearCache();
    }
}