using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using PayamBack.Data;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PayamBack.Services.Implementations
{
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly UserManager<AppUser> _userManager;
        private readonly AppDbContext _context;

        public TokenService(IConfiguration configuration, IMemoryCache cache, UserManager<AppUser> userManager, AppDbContext context)
        {
            _configuration = configuration;
            _cache = cache;
            _userManager = userManager;
            _context = context;
        }

        public async Task<string> GenerateAccessToken(AppUser user)
        {
            // ============================================================
            // 1️⃣ دریافت نقش فعال و MarkazId (با ToListAsync و سپس FirstOrDefault)
            // ============================================================
            var activeRole = await _context.UserRoles
                .Where(ur => ur.UserId == user.Id && ur.RolePishFarz == true)
                .Select(ur => new { ur.RoleId, ur.MarkazId })
                .FirstOrDefaultAsync();

            int? codeRole = null;
            int? markazId = null;
            string? ostanId = null;

            if (activeRole != null)
            {
                // ============================================================
                // 2️⃣ دریافت CodeRole
                // ============================================================
                var role = await _context.Roles
                    .Where(r => r.Id == activeRole.RoleId)
                    .Select(r => r.CodeRole)
                    .FirstOrDefaultAsync();

                codeRole = role;
                markazId = activeRole.MarkazId;

                // ============================================================
                // 3️⃣ دریافت OstanId از Markaz
                // ============================================================
                if (markazId.HasValue)
                {
                    var markaz = await _context.Markazes
                        .Where(m => m.Id == markazId.Value)
                        .Select(m => m.CodeOstan)
                        .FirstOrDefaultAsync();

                    ostanId = markaz;
                }
            }

            // ============================================================
            // 4️⃣ گرفتن نقش‌های کاربر
            // ============================================================
            var roles = await _userManager.GetRolesAsync(user);

            // ============================================================
            // 5️⃣ ساخت Claims
            // ============================================================
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                new Claim("MarkazId", markazId?.ToString() ?? ""),
                new Claim("CodeRole", codeRole?.ToString() ?? "4"),
                new Claim("OstanId", ostanId ?? "")
            };

            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

            // ============================================================
            // 6️⃣ ساخت توکن
            // ============================================================
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
            var expires = DateTime.UtcNow.AddMinutes(Convert.ToDouble(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15"));

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: expires,
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> GenerateRefreshToken(AppUser user)
        {
            var refreshToken = Guid.NewGuid().ToString("N");
            await _userManager.SetAuthenticationTokenAsync(user, "PayamBack", "RefreshToken", refreshToken);

            var cacheKey = $"RefreshToken_{user.Id}";
            _cache.Set(cacheKey, refreshToken, TimeSpan.FromDays(7));

            return refreshToken;
        }

        public async Task<bool> ValidateRefreshToken(AppUser user, string refreshToken)
        {
            var cacheKey = $"RefreshToken_{user.Id}";

            if (_cache.TryGetValue(cacheKey, out string? cachedToken))
            {
                return cachedToken == refreshToken;
            }

            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "PayamBack", "RefreshToken");
            if (string.IsNullOrEmpty(storedToken)) return false;

            _cache.Set(cacheKey, storedToken, TimeSpan.FromDays(7));
            return storedToken == refreshToken;
        }

        public async Task RevokeRefreshToken(AppUser user)
        {
            var cacheKey = $"RefreshToken_{user.Id}";
            _cache.Remove(cacheKey);
            await _userManager.RemoveAuthenticationTokenAsync(user, "PayamBack", "RefreshToken");
        }

        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
        }
    }
}