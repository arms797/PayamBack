// PayamBack/Services/Interfaces/ICacheManager.cs
namespace PayamBack.Services.Interfaces
{
    public interface ICacheManager
    {
        void ClearUserCache(int userId);
        void ClearMarkazCache();
        void ClearPermissionCache(int? roleId = null);
        void ClearAll();
    }
}