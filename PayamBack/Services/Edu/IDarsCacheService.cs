using PayamBack.DTOs.Edu.Dars;

namespace PayamBack.Services.Edu
{
    public interface IDarsCacheService
    {
        Task<IReadOnlyList<DarsCacheDto>> GetAllAsync(CancellationToken cancellationToken = default);
        DarsCacheDto? GetById(int id);
        Task RebuildAsync(CancellationToken cancellationToken = default);
        DateTime? LastRebuiltAt { get; }
    }
}