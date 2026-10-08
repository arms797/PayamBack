using PayamBack.DTOs.Schedule.BarnamehHaftegi;

namespace PayamBack.Services.Interfaces
{
    public interface IPermittedMarkazCacheService
    {
        Task<List<PermittedMarkazInfo>> GetAsync(
            int ostadId,
            string? termCode,
            Func<Task<List<PermittedMarkazInfo>>> factory);

        void Clear(int ostadId, string? termCode);
        void ClearAllForOstad(int ostadId);
    }
}