// PayamBack/Services/Interfaces/IAccessService.cs
namespace PayamBack.Services.Interfaces
{
    public interface IAccessService
    {
        Task<bool> IsOstadUserAsync(int userId);
        Task<bool> CanAccessTargetUserAsync(int targetUserId, int codeRole, int? currentMarkazId);
        Task<bool> CanAccessTargetMarkazAsync(int targetMarkazId, int codeRole, int? currentMarkazId);
        Task<List<int>> GetAccessibleMarkazIdsAsync(int codeRole, int? currentMarkazId);
        Task<int?> GetRoleIdByNameAsync(string roleName);
        Task<bool> CanAccessTargetOstadAsync(int ostadId, int codeRole, int? currentMarkazId);
    }
}