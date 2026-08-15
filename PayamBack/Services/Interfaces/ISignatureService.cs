// Services/Interfaces/ISignatureService.cs
namespace PayamBack.Services.Interfaces
{
    public interface ISignatureService
    {
        /// <summary>
        /// دریافت امضای چند کاربر بر اساس لیست شناسه‌ها
        /// </summary>
        /// <param name="userIds">لیست شناسه کاربران</param>
        /// <returns>دیکشنری با کلید UserId و مقدار Signature (Base64) یا null</returns>
        Task<Dictionary<int, (string? Signature, string? Position)>> GetSignaturesByUserIdsAsync(List<int> userIds);
    }
}