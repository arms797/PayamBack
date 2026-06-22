using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.IdentityModel.Tokens;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace PayamBack.Services.Implementations
{
    /// <summary>
    /// پیاده‌سازی سرویس مدیریت توکن‌ها
    /// RefreshToken در جدول AspNetUserTokens (Identity) و در Cache ذخیره می‌شود
    /// </summary>
    public class TokenService : ITokenService
    {
        private readonly IConfiguration _configuration;
        private readonly IMemoryCache _cache;
        private readonly UserManager<AppUser> _userManager;

        public TokenService(IConfiguration configuration, IMemoryCache cache, UserManager<AppUser> userManager)
        {
            _configuration = configuration;
            _cache = cache;
            _userManager = userManager;
        }

        // ============================================================
        // 1️⃣ ساخت AccessToken (JWT)
        // ============================================================
        public async Task<string> GenerateAccessToken(AppUser user)
        {
            // گرفتن نقش‌های کاربر
            var roles = await _userManager.GetRolesAsync(user);

            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.NameIdentifier, user.Id.ToString()),
                new Claim(ClaimTypes.Name, user.UserName ?? ""),
                new Claim(ClaimTypes.Email, user.Email ?? ""),
                new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
            };

            // اضافه کردن نقش‌ها به Claims
            foreach (var role in roles)
            {
                claims.Add(new Claim(ClaimTypes.Role, role));
            }

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

        // ============================================================
        // 2️⃣ ساخت RefreshToken و ذخیره در Identity + Cache
        // ============================================================
        public async Task<string> GenerateRefreshToken(AppUser user)
        {
            var refreshToken = Guid.NewGuid().ToString("N");

            // ذخیره در Identity (جدول AspNetUserTokens)
            await _userManager.SetAuthenticationTokenAsync(user, "PayamBack", "RefreshToken", refreshToken);

            // ذخیره در Cache برای سرعت
            var cacheKey = $"RefreshToken_{user.Id}";
            _cache.Set(cacheKey, refreshToken, TimeSpan.FromDays(7));

            return refreshToken;
        }

        // ============================================================
        // 3️⃣ اعتبارسنجی RefreshToken (از Cache یا Identity)
        // ============================================================
        public async Task<bool> ValidateRefreshToken(AppUser user, string refreshToken)
        {
            var cacheKey = $"RefreshToken_{user.Id}";

            // 1️⃣ ابتدا از Cache بررسی کن
            if (_cache.TryGetValue(cacheKey, out string? cachedToken))
            {
                return cachedToken == refreshToken;
            }

            // 2️⃣ اگر در Cache نبود، از Identity بخوان
            var storedToken = await _userManager.GetAuthenticationTokenAsync(user, "PayamBack", "RefreshToken");

            if (string.IsNullOrEmpty(storedToken))
                return false;

            // 3️⃣ اگر در Identity پیدا شد، دوباره در Cache ذخیره کن
            _cache.Set(cacheKey, storedToken, TimeSpan.FromDays(7));

            return storedToken == refreshToken;
        }

        // ============================================================
        // 4️⃣ باطل کردن RefreshToken (حذف از Cache و Identity)
        // ============================================================
        public async Task RevokeRefreshToken(AppUser user)
        {
            // 1️⃣ حذف از Cache
            var cacheKey = $"RefreshToken_{user.Id}";
            _cache.Remove(cacheKey);

            // 2️⃣ حذف از Identity
            await _userManager.RemoveAuthenticationTokenAsync(user, "PayamBack", "RefreshToken");
        }

        // ============================================================
        // 5️⃣ خواندن اطلاعات از AccessToken منقضی شده
        // ============================================================
        public ClaimsPrincipal GetPrincipalFromExpiredToken(string token)
        {
            var tokenValidationParameters = new TokenValidationParameters
            {
                ValidateIssuer = true,
                ValidateAudience = true,
                ValidateLifetime = false,  // مهم: برای خواندن توکن منقضی شده
                ValidateIssuerSigningKey = true,
                ValidIssuer = _configuration["Jwt:Issuer"],
                ValidAudience = _configuration["Jwt:Audience"],
                IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_configuration["Jwt:Key"]!))
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var principal = tokenHandler.ValidateToken(token, tokenValidationParameters, out _);
            return principal;
        }
    }
}