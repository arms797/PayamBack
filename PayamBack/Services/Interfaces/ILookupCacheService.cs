using PayamBack.Models.Edu;
using PayamBack.Models.Schedule;
using PayamBack.DTOs.Lookup;

namespace PayamBack.Services.Interfaces
{
    public interface ILookupCacheService
    {
        Task<List<WeekDay>> GetActiveDaysAsync();
        Task<List<SaatBargozariKelasha>> GetActiveHoursAsync();
        Task<List<Faaliat>> GetActiveFaaliatAsync();
        Task<List<HaftegiExceptionDto>> GetActiveExceptionsAsync();
        Task<LookupData> GetAllAsync();
        void ClearCache();
    }

    public class LookupData
    {
        public List<WeekDay> Days { get; set; } = new();
        public List<SaatBargozariKelasha> Hours { get; set; } = new();
        public List<Faaliat> Faaliats { get; set; } = new();
        public List<FaaliatGroup> FaaliatGroups { get; set; } = new();
        public List<HaftegiExceptionDto> HaftegiExceptions { get; set; } = new();
    }
}