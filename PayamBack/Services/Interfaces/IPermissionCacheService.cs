// PayamBack/Services/Interfaces/IPermissionCacheService.cs
namespace PayamBack.Services.Interfaces
{
    public interface IPermissionCacheService
    {
        Task<List<string>> GetRolePermissionsAsync(int roleId);
        void ClearRoleCache(int roleId);
        void ClearAllCache();
    }
}