// Services/Implementations/SignatureService.cs
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Services.Interfaces;

namespace PayamBack.Services.Implementations
{
    public class SignatureService : ISignatureService
    {
        private readonly AppDbContext _context;

        public SignatureService(AppDbContext context)
        {
            _context = context;
        }

        // Services/Implementations/SignatureService.cs
        public async Task<Dictionary<int, (string? Signature, string? Position)>> GetSignaturesByUserIdsAsync(List<int> userIds)
        {
            try
            {
                if (userIds == null || !userIds.Any())
                    return new Dictionary<int, (string? Signature, string? Position)>();

                var uniqueUserIds = userIds.Distinct().ToList();

                var signatures = await _context.UserSignatures
                    .Where(s => uniqueUserIds.Contains(s.UserId))
                    .ToDictionaryAsync(
                        s => s.UserId,
                        s => (s.Signature as string, s.Position as string)
                    );

                var result = new Dictionary<int, (string? Signature, string? Position)>();
                foreach (var userId in uniqueUserIds)
                {
                    result[userId] = signatures.TryGetValue(userId, out var sig) ? sig : (null, null);
                }

                return result;
            }
            catch (Exception ex)
            {
                // در صورت خطا، دیکشنری خالی برگردان
                return new Dictionary<int, (string? Signature, string? Position)>();
            }
        }
    }
}