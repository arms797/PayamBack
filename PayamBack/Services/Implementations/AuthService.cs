using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Services.Implementations
{
    public class AuthService : IAuthService
    {
        private readonly UserManager<AppUser> _userManager;
        private readonly SignInManager<AppUser> _signInManager;
        private readonly ITokenService _tokenService;
        private readonly IPermissionService _permissionService;
        private readonly IConfiguration _configuration;
        private readonly AppDbContext _context;
        private readonly ICaptchaService _captchaService;

        public AuthService(
            UserManager<AppUser> userManager,
            SignInManager<AppUser> signInManager,
            ITokenService tokenService,
            IPermissionService permissionService,
            IConfiguration configuration,
            AppDbContext context,
            ICaptchaService captchaService)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _tokenService = tokenService;
            _permissionService = permissionService;
            _configuration = configuration;
            _context = context;
            _captchaService = captchaService;
        }

        // ============================================================
        // 1️⃣ ورود کاربر
        // ============================================================
        public async Task<LoginResponseDto> LoginAsync(LoginRequestDto request)
        {
            // اعتبارسنجی CAPTCHA
            if (string.IsNullOrEmpty(request.CaptchaKey) || string.IsNullOrEmpty(request.CaptchaAnswer))
            {
                throw new Exception("captcha_required");
            }

            var isValidCaptcha = _captchaService.ValidateCaptcha(request.CaptchaKey, request.CaptchaAnswer);
            if (!isValidCaptcha)
            {
                throw new Exception("captcha_invalid");
            }
            _captchaService.RemoveCaptcha(request.CaptchaKey);

            // بررسی کاربر
            var user = await _userManager.FindByNameAsync(request.Username);
            if (user == null)
            {
                throw new Exception("login_invalid");
            }

            // بررسی رمز عبور
            var result = await _signInManager.CheckPasswordSignInAsync(user, request.Password, false);
            if (!result.Succeeded)
            {
                throw new Exception("login_invalid");
            }

            // تولید توکن‌ها
            var accessToken = await _tokenService.GenerateAccessToken(user);
            var refreshToken = await _tokenService.GenerateRefreshToken(user);

            // ============================================================
            // دریافت نقش‌های کاربر
            // ============================================================
            var roles = await _permissionService.GetUserRolesAsync(user.Id);

            // ============================================================
            // 🔥 تعیین نقش فعال (پیش‌فرض یا اولین نقش)
            // ============================================================
            RoleDto? activeRole = roles.FirstOrDefault(r => r.IsDefault);

            // اگر نقش پیش‌فرض وجود نداشت، اولین نقش را انتخاب کن
            if (activeRole == null && roles.Any())
            {
                activeRole = roles.First();
            }

            // ============================================================
            // دریافت منوها بر اساس نقش فعال
            // ============================================================
            var menus = activeRole != null
                ? await _permissionService.GetUserMenusAsync(user.Id, activeRole.Id)
                : new List<MenuDto>();

            // ============================================================
            // 🔥 دریافت نام و نام خانوادگی کاربر
            // ============================================================
            string firstName = "";
            string lastName = "";

            // اگر کاربر استاد است
            if (user.OstadId.HasValue)
            {
                var ostad = await _context.Ostads.FindAsync(user.OstadId.Value);
                if (ostad != null)
                {
                    firstName = ostad.Naam ?? "";
                    lastName = ostad.NaamKhanevadegi ?? "";
                }
            }
            // اگر کاربر کارمند است
            else if (user.KarmandId.HasValue)
            {
                var karmand = await _context.Karmands.FindAsync(user.KarmandId.Value);
                if (karmand != null)
                {
                    firstName = karmand.Naam ?? "";
                    lastName = karmand.NaameKhanevadeghi ?? "";
                }
            }
            // اگر کاربر دانشجو است
            else if (user.DaneshjooId.HasValue)
            {
                var daneshjoo = await _context.Daneshjoos.FindAsync(user.DaneshjooId.Value);
                if (daneshjoo != null)
                {
                    firstName = daneshjoo.Naam ?? "";
                    lastName = daneshjoo.NaamKhanevadegi ?? "";
                }
            }
            // اگر کاربر ادمین است (از جدول MoshakhasatAdmin)
            else
            {
                var admin = await _context.MoshakhasatAdmins
                    .FirstOrDefaultAsync(m => m.Email == user.Email);
                if (admin != null)
                {
                    firstName = admin.Naam ?? "";
                    lastName = admin.NaameKhanevadeghi ?? "";
                }
            }

            // ============================================================
            // برگرداندن پاسخ
            // ============================================================
            return new LoginResponseDto
            {
                AccessToken = accessToken,
                RefreshToken = refreshToken,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                FirstName = firstName,
                LastName = lastName,
                CurrentRoleId = activeRole?.Id,
                CurrentRoleName = activeRole?.Name ?? "",
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
            var principal = _tokenService.GetPrincipalFromExpiredToken(request.AccessToken);
            var userIdClaim = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                throw new Exception("توکن نامعتبر");

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("کاربر یافت نشد");

            var isValid = await _tokenService.ValidateRefreshToken(user, request.RefreshToken);
            if (!isValid)
                throw new Exception("RefreshToken نامعتبر است");

            var newAccessToken = await _tokenService.GenerateAccessToken(user);
            var newRefreshToken = await _tokenService.GenerateRefreshToken(user);

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
        // 3️⃣ خروج کاربر
        // ============================================================
        public async Task<bool> LogoutAsync(int userId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return false;

            await _tokenService.RevokeRefreshToken(user);
            return true;
        }

        // ============================================================
        // 4️⃣ تغییر نقش فعال کاربر
        // ============================================================
        public async Task<LoginResponseDto> ChangeRoleAsync(int userId, int newRoleId)
        {
            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                throw new Exception("کاربر یافت نشد");

            var userRoles = await _permissionService.GetUserRolesAsync(userId);
            if (!userRoles.Any(r => r.Id == newRoleId))
                throw new Exception("شما به این نقش دسترسی ندارید");

            var userRole = await _context.Set<AppUserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == userId && ur.RoleId == newRoleId);

            if (userRole != null)
            {
                var allUserRoles = await _context.Set<AppUserRole>()
                    .Where(ur => ur.UserId == userId)
                    .ToListAsync();

                foreach (var ur in allUserRoles)
                {
                    ur.RolePishFarz = false;
                }

                userRole.RolePishFarz = true;
                await _context.SaveChangesAsync();
            }

            var roles = await _permissionService.GetUserRolesAsync(userId);
            var newRole = roles.FirstOrDefault(r => r.Id == newRoleId);
            var menus = await _permissionService.GetUserMenusAsync(userId, newRoleId);

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