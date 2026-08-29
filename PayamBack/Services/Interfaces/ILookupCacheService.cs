using PayamBack.Models.Edu;
using PayamBack.Models.Schedule;

namespace PayamBack.Services.Interfaces
{
    public interface ILookupCacheService
    {
        Task<List<WeekDay>> GetActiveDaysAsync();
        Task<List<SaatBargozariKelasha>> GetActiveHoursAsync();
        Task<List<Faaliat>> GetActiveFaaliatAsync();
        Task<LookupData> GetAllAsync();
        void ClearCache();
    }

    public class LookupData
    {
        public List<WeekDay> Days { get; set; } = new();
        public List<SaatBargozariKelasha> Hours { get; set; } = new();
        public List<Faaliat> Faaliats { get; set; } = new();
    }
}