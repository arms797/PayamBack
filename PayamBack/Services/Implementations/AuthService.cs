using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Services.Implementations
{
    /// <summary>
    /// پیاده‌سازی سرویس احراز هویت
    /// </summary>
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IPermissionService permissionService,
            IConfiguration configuration,
            AppDbContext context)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _permissionService = permissionService;
            _configuration = configuration;
            _context = context;
        }

        // ============================================================
        // 1️⃣ ورود کاربر
        // ============================================================
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // پیدا کردن کاربر با نام کاربری
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
                throw new UnauthorizedAccessException("نام کاربری یا رمز عبور اشتباه است");

            // بررسی رمز عبور
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
                throw new UnauthorizedAccessException("نام کاربری یا رمز عبور اشتباه است");

            // 1️⃣ تولید AccessToken
            var accessToken = await _tokenService.GenerateAccessToken(user);

            // 2️⃣ تولید RefreshToken و ذخیره در Identity + Cache
            var refreshToken = await _tokenService.GenerateRefreshToken(user);

            // 3️⃣ گرفتن همه نقش‌های کاربر
            var roles = await _permissionService.GetUserRolesAsync(user.Id);

            // 4️⃣ گرفتن نقش پیش‌فرض
            var defaultRole = roles.FirstOrDefault(r => r.IsDefault);

            // 5️⃣ گرفتن منوهای قابل نمایش بر اساس نقش پیش‌فرض
            var menus = defaultRole != null
                ? await _permissionService.GetUserMenusAsync(user.Id, defaultRole.Id)
                : new List<MenuDto>();

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                CurrentRoleId = defaultRole?.Id,
                CurrentRoleName = defaultRole?.Name ?? "",
                Roles = roles,
                Menus = menus,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15")
            };
        }

        // ============================================================
        // 2️⃣ تمدید AccessToken با RefreshToken
        // ============================================================
        public async Task<LoginResponseDto> RefreshTokenAsync(RefreshTokenRequestDto request)
        {
            // 1️⃣ خواندن اطلاعات از AccessToken منقضی شده
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);

            // 2️⃣ گرفتن شناسه کاربر از توکن
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new UnauthorizedAccessException("توکن نامعتبر");

            // 3️⃣ پیدا کردن کاربر
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new UnauthorizedAccessException("کاربر یافت نشد");

            // 4️⃣ اعتبارسنجی RefreshToken (از Cache یا Identity)
            var isValid = await _tokenService.ValidateRefreshToken(user, request.RefreshToken);
            if (!isValid)
                throw new UnauthorizedAccessException("RefreshToken نامعتبر است");

            // 5️⃣ تولید توکن‌های جدید
            var newAccessToken = await _tokenService.GenerateAccessToken(user);
            var newRefreshToken = await _tokenService.GenerateRefreshToken(user);

            // 6️⃣ گرفتن نقش‌ها و منوها
            var roles = await _permissionService.GetUserRolesAsync(user.Id);
            var defaultRole = roles.FirstOrDefault(r => r.IsDefault);
            var menus = defaultRole != null
                ? await _permissionService.GetUserMenusAsync(user.Id, defaultRole.Id)
                : new List<MenuDto>();

            return new LoginResponseDto
            {
                AccessToken = newAccessToken,
                RefreshToken = newRefreshToken,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                CurrentRoleId = defaultRole?.Id,
                CurrentRoleName = defaultRole?.Name ?? "",
                Roles = roles,
                Menus = menus,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15")
            };
        }

        // ============================================================
        // 3️⃣ خروج کاربر (باطل کردن RefreshToken)
        // ============================================================
        public async Task<bool> LogoutAsync(int userId)
        {
            // پیدا کردن کاربر
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;

            // باطل کردن RefreshToken
            await _tokenService.RevokeRefreshToken(user);

            return true;
        }

        // ============================================================
        // 4️⃣ تغییر نقش فعال کاربر
        // ============================================================
        public async Task<LoginResponseDto> ChangeRoleAsync(int userId, int newRoleId)
        {
            // 1️⃣ پیدا کردن کاربر
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new UnauthorizedAccessException("کاربر یافت نشد");

            // 2️⃣ بررسی اینکه کاربر این نقش را دارد
            var userRoles = await _permissionService.GetUserRolesAsync(userId);
            if (!userRoles.Any(r => r.Id == newRoleId))
                throw new UnauthorizedAccessException("شما به این نقش دسترسی ندارید");

            // 3️⃣ تغییر نقش پیش‌فرض در جدول AppUserRole
            var userRole = await _context.Set<AppUserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == newRoleId);

            if (userRole != null)
            {
                // همه نقش‌های کاربر را غیرفعال کن
                var allUserRoles = await _context.Set<AppUserRole>()
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();

                foreach (var ur in allUserRoles)
                {
                    ur.RolePishFarz = false;
                }

                // نقش جدید را فعال کن
                userRole.RolePishFarz = true;
                await _context.SaveChangesAsync();
            }

            // 4️⃣ گرفتن نقش‌ها و منوهای جدید
            var roles = await _permissionService.GetUserRolesAsync(userId);
            var newRole = roles.FirstOrDefault(r => r.Id == newRoleId);
            var menus = await _permissionService.GetUserMenusAsync(userId, newRoleId);

            // 5️⃣ تولید توکن جدید (برای امنیت بیشتر)
            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshToken(user);

            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                CurrentRoleId = newRole?.Id,
                CurrentRoleName = newRole?.Name ?? "",
                Roles = roles,
                Menus = menus,
                ExpiresIn = Convert.ToInt32(_configuration["Jwt:AccessTokenExpiryMinutes"] ?? "15")
            };
        }
    }
}