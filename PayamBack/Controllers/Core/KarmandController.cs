using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Karmand;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using System.Security.Claims;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class KarmandController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;

        public KarmandController(AppDbContext context, UserManager<AppUser> userManager, RoleManager<AppRole> roleManager)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // ============================================================
        // 🔥 متد کمکی برای دریافت اطلاعات کاربر فعلی و نقش فعال
        // ============================================================
        private async Task<(AppUser? user, AppRole? role, Markaz? markaz, int? codeRole)> GetCurrentUserInfoAsync()
        {
            var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdClaim) || !int.TryParse(userIdClaim, out int userId))
                return (null, null, null, null);

            var user = await _userManager.FindByIdAsync(userId.ToString());
            if (user == null)
                return (null, null, null, null);

            // دریافت نقش فعال از JWT
            var roleName = User.FindFirst(ClaimTypes.Role)?.Value;
            if (string.IsNullOrEmpty(roleName))
                return (user, null, null, null);

            var role = await _roleManager.FindByNameAsync(roleName);
            if (role == null)
                return (user, null, null, null);

            // دریافت مرکز نقش فعال
            //var activeRole = await _context.Set<AppUserRole>()
            //    .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id &&);
            var activeRole = await _context.Set<AppUserRole>()
                .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id &&
                ur.RolePishFarz == true);
            if (activeRole == null)
            {
                activeRole = await _context.Set<AppUserRole>()
                    .FirstOrDefaultAsync(ur => ur.UserId == user.Id && ur.RoleId == role.Id);
            }
            Markaz? markaz = null;
            if (activeRole?.MarkazId != null)
            {
                markaz = await _context.Markazes.FindAsync(activeRole.MarkazId.Value);
            }

            return (user, role, markaz, role.CodeRole);
        }

        // ============================================================
        // 🔥 متد کمکی برای بررسی دسترسی به مرکز هدف
        // ============================================================
        private async Task<bool> CanAccessTargetMarkazAsync(int targetMarkazId, int codeRole, int? currentMarkazId)
        {
            // ادمین سامانه (کد 1) → همه مراکز
            if (codeRole == 1)
                return true;

            var targetMarkaz = await _context.Markazes.FindAsync(targetMarkazId);
            if (targetMarkaz == null)
                return false;

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null)
                return false;

            // ادمین سازمان (کد 2) → فقط سازمان خود (Level 2) و استان‌ها (Level 3)
            if (codeRole == 2)
            {
                return targetMarkaz.Level == 2 || targetMarkaz.Level == 3;
            }

            // ادمین استان (کد 3) → فقط استان خود (Level 3) و مراکز آن استان (Level 4)
            if (codeRole == 3)
            {
                return targetMarkaz.Level == 3 || (targetMarkaz.Level == 4 && targetMarkaz.CodeOstan == currentMarkaz.CodeOstan);
            }

            // ادمین مرکز (کد 4) → فقط مرکز خود (Level 4)
            if (codeRole == 4)
            {
                return targetMarkaz.Id == currentMarkaz.Id;
            }

            return false;
        }

        // ============================================================
        // 🔥 متد کمکی برای گرفتن مراکز قابل دسترس
        // ============================================================
        private async Task<List<int>> GetAccessibleMarkazIdsAsync(int codeRole, int? currentMarkazId)
        {
            if (codeRole == 1)
            {
                // ادمین سامانه: همه مراکز فعال
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true)
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            var currentMarkaz = await _context.Markazes.FindAsync(currentMarkazId);
            if (currentMarkaz == null)
                return new List<int>();

            if (codeRole == 2)
            {
                // ادمین سازمان: فقط مراکز با Level 2 و 3
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true && (m.Level == 2 || m.Level == 3))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            if (codeRole == 3)
            {
                // ادمین استان: استان خود و مراکز آن استان
                return await _context.Markazes
                    .Where(m => m.Vazeeyat == true &&
                        (m.Level == 3 && m.CodeOstan == currentMarkaz.CodeOstan) ||
                        (m.Level == 4 && m.CodeOstan == currentMarkaz.CodeOstan))
                    .Select(m => m.Id)
                    .ToListAsync();
            }

            if (codeRole == 4)
            {
                // ادمین مرکز: فقط مرکز خود
                return new List<int> { currentMarkaz.Id };
            }

            return new List<int>();
        }

        // ============================================================
        // 1️⃣ دریافت لیست کارمندان با صفحه‌بندی و فیلتر
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 50,
            [FromQuery] string? search = null,
            [FromQuery] int? ostanId = null,
            [FromQuery] int? markazId = null,
            [FromQuery] int? vazeeat = 3)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و مراکز قابل دسترس
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var accessibleMarkazIds = await GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                if (!accessibleMarkazIds.Any())
                    return Ok(new { success = true, message = "شما دسترسی به هیچ مرکزی ندارید", data = new List<object>(), pagination = new { page, pageSize, totalCount = 0, totalPages = 0 } });

                // ============================================================
                // 1️⃣ ساخت کوئری پایه با Join به AppUser برای دریافت UserId و وضعیت
                // ============================================================
                var query = from k in _context.Karmands
                            join u in _context.Users on k.Id equals u.KarmandId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where k.MarkazId.HasValue //&& accessibleMarkazIds.Contains(k.MarkazId.Value)
                            select new { Karmand = k, User = u };

                // ============================================================
                // 2️⃣ فیلتر بر اساس جستجو
                // ============================================================
                if (!string.IsNullOrEmpty(search))
                {
                    query = query.Where(x =>
                        (x.Karmand.Naam != null && x.Karmand.Naam.Contains(search)) ||
                        (x.Karmand.NaameKhanevadeghi != null && x.Karmand.NaameKhanevadeghi.Contains(search)) ||
                        (x.Karmand.CodeMelli != null && x.Karmand.CodeMelli.Contains(search)));
                }

                // ============================================================
                // 3️⃣ فیلتر بر اساس استان و مرکز
                // ============================================================
                if (ostanId.HasValue && !markazId.HasValue)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(x =>
                        x.Karmand.MarkazId.HasValue &&
                        markazIdsInOstan.Contains(x.Karmand.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue)
                {
                    query = query.Where(x => x.Karmand.MarkazId == markazId.Value);
                }

                // ============================================================
                // 4️⃣ فیلتر بر اساس وضعیت
                // ============================================================
                //if (vazeeat.HasValue)
                //{
                    if (vazeeat == 1)
                    {
                        query = query.Where(x => x.User != null &&
                            (x.User.Vazeeyat == true && x.User.VazeeyatMovaghat == true));
                    }
                    else if(vazeeat==2)
                    {
                        query = query.Where(x => x.User != null &&
                            (x.User.Vazeeyat == false || x.User.VazeeyatMovaghat == false));
                    }
                else
                {
                    query = query.Where(x => x.User != null);
                }

                //}
                //else
                //{
                //query = query.Where(x => x.User == null || x.User.Vazeeyat == true);
                //}

                // ============================================================
                // 5️⃣ محاسبه تعداد کل
                // ============================================================
                var totalCount = await query.CountAsync();

                // ============================================================
                // 6️⃣ صفحه‌بندی
                // ============================================================
                var karmands = await query
                    .OrderBy(x => x.Karmand.NaameKhanevadeghi)
                    .ThenBy(x => x.Karmand.Naam)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new KarmandListDto
                    {
                        Id = x.Karmand.Id,
                        // ============================================================
                        // 🔥 اضافه کردن UserId
                        // ============================================================
                        UserId = x.User != null ? x.User.Id : (int?)null,
                        CodeMelli = x.Karmand.CodeMelli ?? "",
                        Naam = x.Karmand.Naam ?? "",
                        NaameKhanevadeghi = x.Karmand.NaameKhanevadeghi ?? "",
                        MarkazId = x.Karmand.MarkazId ?? 0,
                        MarkazName = _context.Markazes
                            .Where(m => m.Id == x.Karmand.MarkazId)
                            .Select(m => m.NaamMarkaz ?? "")
                            .FirstOrDefault() ?? "",
                        Mobile = x.Karmand.Mobile ?? "",
                        Email = x.Karmand.Email ?? "",
                        Vazeeat = x.User != null ? x.User.Vazeeyat ?? true : true,
                        VazeeatMovaghat = x.User != null ? x.User.VazeeyatMovaghat ?? false : false
                    })
                    .ToListAsync();

                // ============================================================
                // 7️⃣ برگرداندن پاسخ
                // ============================================================
                return Ok(new
                {
                    success = true,
                    message = "لیست کارمندان دریافت شد",
                    data = karmands,
                    pagination = new
                    {
                        page,
                        pageSize,
                        totalCount,
                        totalPages = (int)Math.Ceiling((double)totalCount / pageSize)
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت کارمندان",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک کارمند
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var karmand = await _context.Karmands
                    .Include(k => k.Markaz)
                    .Include(k => k.MarkazAsli)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                var dto = new KarmandDetailDto
                {
                    Id = karmand.Id,
                    CodeMelli = karmand.CodeMelli ?? "",
                    Naam = karmand.Naam ?? "",
                    NaameKhanevadeghi = karmand.NaameKhanevadeghi ?? "",
                    MarkazId = karmand.MarkazId ?? 0,
                    MarkazName = karmand.Markaz?.NaamMarkaz ?? "",
                    MarkazAsliId = karmand.MarkazAsliId ?? 0,
                    MarkazAsliName = karmand.MarkazAsli?.NaamMarkaz ?? "",
                    Mobile = karmand.Mobile ?? "",
                    Mobile2 = karmand.Mobile2 ?? "",
                    TelefonMostaghim = karmand.TelefonMostaghim ?? "",
                    TelefonGhayreMostaghim = karmand.TelefonGhayreMostaghim ?? "",
                    TelefonDakheli = karmand.TelefonDakheli ?? "",
                    Email = karmand.Email ?? "",
                    Emza = karmand.Emza ?? ""
                };

                return Ok(new { success = true, message = "اطلاعات کارمند دریافت شد", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد کارمند جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] KarmandCreateDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // بررسی دسترسی به مرکز هدف
                if (!await CanAccessTargetMarkazAsync(dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                {
                    return Forbid();
                }

                // ============================================================
                // 1️⃣ اعتبارسنجی ورودی
                // ============================================================
                // بررسی کد ملی تکراری
                var exists = await _context.Karmands
                    .AnyAsync(k => k.CodeMelli == dto.CodeMelli);

                if (exists)
                    return BadRequest(new { success = false, message = "کد ملی قبلاً ثبت شده است" });

                // بررسی تکراری بودن نام کاربری
                var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "نام کاربری قبلاً ثبت شده است" });

                // ============================================================
                // 2️⃣ ایجاد کارمند
                // ============================================================
                var karmand = new Karmand
                {
                    CodeMelli = dto.CodeMelli,
                    Naam = dto.Naam,
                    NaameKhanevadeghi = dto.NaameKhanevadeghi,
                    MarkazId = dto.MarkazId,
                    MarkazAsliId = dto.MarkazAsliId,
                    Mobile = dto.Mobile,
                    Mobile2 = dto.Mobile2,
                    TelefonMostaghim = dto.TelefonMostaghim,
                    TelefonGhayreMostaghim = dto.TelefonGhayreMostaghim,
                    TelefonDakheli = dto.TelefonDakheli,
                    Email = dto.Email,
                    Emza = dto.Emza
                };

                await _context.Karmands.AddAsync(karmand);
                await _context.SaveChangesAsync();

                // ============================================================
                // 3️⃣ ایجاد کاربر متناظر
                // ============================================================
                var user = new AppUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    KarmandId = karmand.Id,
                    Vazeeyat = true,
                    VazeeyatMovaghat = true
                };

                var password = dto.CodeMelli+"aA";
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _context.Karmands.Remove(karmand);
                    await _context.SaveChangesAsync();

                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ایجاد کاربر",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // ============================================================
                // 4️⃣ اضافه کردن نقش (در صورت وجود)
                // ============================================================
               /* if (!string.IsNullOrEmpty(dto.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, dto.RoleName);

                    // ثبت در AppUserRole
                    var appUserRole = new AppUserRole
                    {
                        UserId = user.Id,
                        RoleId = (await _roleManager.FindByNameAsync(dto.RoleName))?.Id ?? 0,
                        MarkazId = dto.MarkazId,
                        RolePishFarz = true
                    };
                    await _context.Set<AppUserRole>().AddAsync(appUserRole);
                    await _context.SaveChangesAsync();
                }*/

                return Ok(new
                {
                    success = true,
                    message = "کارمند با موفقیت ایجاد شد",
                    data = new { karmandId = karmand.Id, userId = user.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش کارمند
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] KarmandUpdateDto dto)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var karmand = await _context.Karmands
                    .Include(k => k.Markaz)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                // بررسی دسترسی به مرکز هدف
                if (!await CanAccessTargetMarkazAsync(karmand.MarkazId ?? 0, codeRole.Value, currentMarkaz?.Id))
                {
                    return Forbid();
                }

                // ============================================================
                // 2️⃣ به‌روزرسانی اطلاعات کارمند
                // ============================================================
                karmand.Naam = dto.Naam ?? karmand.Naam;
                karmand.NaameKhanevadeghi = dto.NaameKhanevadeghi ?? karmand.NaameKhanevadeghi;
                karmand.MarkazId = dto.MarkazId ?? karmand.MarkazId;
                karmand.MarkazAsliId = dto.MarkazAsliId ?? karmand.MarkazAsliId;
                karmand.Mobile = dto.Mobile ?? karmand.Mobile;
                karmand.Mobile2 = dto.Mobile2 ?? karmand.Mobile2;
                karmand.TelefonMostaghim = dto.TelefonMostaghim ?? karmand.TelefonMostaghim;
                karmand.TelefonGhayreMostaghim = dto.TelefonGhayreMostaghim ?? karmand.TelefonGhayreMostaghim;
                karmand.TelefonDakheli = dto.TelefonDakheli ?? karmand.TelefonDakheli;
                karmand.Email = dto.Email ?? karmand.Email;
                karmand.Emza = dto.Emza ?? karmand.Emza;

                await _context.SaveChangesAsync();

                // به‌روزرسانی ایمیل کاربر
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.KarmandId == id);

                if (user != null && !string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { success = true, message = "کارمند ویرایش شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 5️⃣ حذف کارمند (فقط ادمین سامانه)
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

                var karmand = await _context.Karmands.FindAsync(id);
                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.KarmandId == id);

                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    // حذف نقش‌های کاربر
                    if (user != null)
                    {
                        var userRoles = await _context.Set<AppUserRole>()
                            .Where(ur => ur.UserId == user.Id)
                            .ToListAsync();

                        if (userRoles.Any())
                        {
                            _context.Set<AppUserRole>().RemoveRange(userRoles);
                            await _context.SaveChangesAsync();
                        }

                        await _userManager.DeleteAsync(user);
                    }

                    _context.Karmands.Remove(karmand);
                    await _context.SaveChangesAsync();

                    await transaction.CommitAsync();

                    return Ok(new { success = true, message = "کارمند و کاربر مربوطه حذف شد" });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف کارمند", error = ex.Message });
            }
        }

        // ============================================================
        // 6️⃣ تغییر وضعیت گروهی کاربران (فعال/غیرفعال) - فقط ادمین سامانه
        // ============================================================
        [HttpPatch("toggle")]
        public async Task<IActionResult> Toggle([FromBody] List<ToggleUserStatusItemDto> items)
        {
            try
            {
                // ============================================================
                // 🔥 دریافت اطلاعات کاربر فعلی و بررسی دسترسی
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 🔥 فقط ادمین سامانه (CodeRole == 1) اجازه دارد
                // ============================================================
                if (codeRole != 1)
                    return Forbid();

                // ============================================================
                // 1️⃣ اعتبارسنجی ورودی
                // ============================================================
                if (items == null || !items.Any())
                    return BadRequest(new { success = false, message = "لیست کاربران خالی است" });

                // بررسی تکراری بودن UserId
                var duplicateUserIds = items
                    .GroupBy(x => x.UserId)
                    .Where(g => g.Count() > 1)
                    .Select(g => g.Key)
                    .ToList();

                if (duplicateUserIds.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "شناسه کاربران تکراری است",
                        duplicateUserIds
                    });
                }

                // بررسی اینکه حداقل یکی از وضعیت‌ها برای هر کاربر مقدار داشته باشد
                var invalidItems = items
                    .Where(x => x.Vazeeyat == null && x.VazeeyatMovaghat == null)
                    .Select(x => x.UserId)
                    .ToList();

                if (invalidItems.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "برای هر کاربر حداقل یکی از وضعیت‌ها باید مقدار داشته باشد",
                        invalidUserIds = invalidItems
                    });
                }

                // ============================================================
                // 2️⃣ دریافت کاربران هدف همراه با اطلاعات کارمند و مرکز
                // ============================================================
                var userIds = items.Select(x => x.UserId).Distinct().ToList();

                var targetUsers = await _userManager.Users
                    .Include(u => u.Karmand)
                        .ThenInclude(k => k.Markaz)
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                if (!targetUsers.Any())
                    return NotFound(new { success = false, message = "هیچ کاربری یافت نشد" });

                // ============================================================
                // 3️⃣ چون ادمین سامانه است، نیازی به بررسی دسترسی مرکز ندارد
                //     ولی کاربرانی که کارمند نیستند را جدا می‌کنیم
                // ============================================================
                var validItems = new List<(ToggleUserStatusItemDto item, AppUser user)>();
                var notFoundUsers = new List<int>();
                var notEmployeeUsers = new List<int>();

                foreach (var item in items)
                {
                    var user = targetUsers.FirstOrDefault(u => u.Id == item.UserId);
                    if (user == null)
                    {
                        notFoundUsers.Add(item.UserId);
                        continue;
                    }

                    // اگر کاربر کارمند نیست، از لیست خارج می‌شود
                    if (user.KarmandId == null)
                    {
                        notEmployeeUsers.Add(item.UserId);
                        continue;
                    }

                    validItems.Add((item, user));
                }

                if (!validItems.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "هیچ کاربر معتبری برای تغییر وضعیت وجود ندارد",
                        notFound = notFoundUsers.Any() ? notFoundUsers : null,
                        notEmployee = notEmployeeUsers.Any() ? notEmployeeUsers : null
                    });
                }

                // ============================================================
                // 4️⃣ تغییر وضعیت کاربران مجاز
                // ============================================================
                var updatedUsers = new List<object>();
                var failedUsers = new List<object>();

                foreach (var (item, user) in validItems)
                {
                    try
                    {
                        if (item.Vazeeyat.HasValue)
                        {
                            user.Vazeeyat = item.Vazeeyat.Value;
                        }

                        if (item.VazeeyatMovaghat.HasValue)
                        {
                            user.VazeeyatMovaghat = item.VazeeyatMovaghat.Value;
                        }

                        var result = await _userManager.UpdateAsync(user);

                        if (result.Succeeded)
                        {
                            updatedUsers.Add(new
                            {
                                userId = user.Id,
                                userName = user.UserName,
                                karmandId = user.KarmandId,
                                vazeeyat = user.Vazeeyat,
                                vazeeyatMovaghat = user.VazeeyatMovaghat
                            });
                        }
                        else
                        {
                            failedUsers.Add(new
                            {
                                userId = user.Id,
                                userName = user.UserName,
                                errors = result.Errors.Select(e => e.Description)
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        failedUsers.Add(new
                        {
                            userId = user.Id,
                            userName = user.UserName,
                            error = ex.Message
                        });
                    }
                }

                // ============================================================
                // 5️⃣ پاسخ نهایی
                // ============================================================
                var response = new
                {
                    success = true,
                    message = $"تعداد {updatedUsers.Count} کاربر با موفقیت به‌روزرسانی شدند",
                    data = new
                    {
                        updated = updatedUsers,
                        failed = failedUsers.Any() ? failedUsers : null,
                        notFound = notFoundUsers.Any() ? notFoundUsers : null,
                        notEmployee = notEmployeeUsers.Any() ? notEmployeeUsers : null
                    }
                };

                return Ok(response);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت کاربران",
                    error = ex.Message
                });
            }
        }

        // در KarmandController.cs

        // ============================================================
        // دریافت UserId متناظر با KarmandId
        // ============================================================
        /*[HttpGet("user-by-karmand/{karmandId}")]
        public async Task<IActionResult> GetUserByKarmandId(int karmandId)
        {
            try
            {
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.KarmandId == karmandId);

                if (user == null)
                    return NotFound(new { success = false, message = "کاربری برای این کارمند یافت نشد" });

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات کاربر دریافت شد",
                    data = new
                    {
                        id = user.Id,
                        userName = user.UserName,
                        email = user.Email,
                        vazeeyat = user.Vazeeyat,
                        vazeeyatMovaghat = user.VazeeyatMovaghat
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات کاربر",
                    error = ex.Message
                });
            }
        }
        // در UserController.cs یا هر کنترلر مناسب

        [HttpPost("reset-password/{userId}")]
        //[Authorize(Roles = "ادمین سامانه,ادمین سازمان,ادمین استان,ادمین مرکز")]
        public async Task<IActionResult> ResetPassword(int userId, [FromBody] ResetPasswordDto dto)
        {
            try
            {
                var user = await _userManager.FindByIdAsync(userId.ToString());
                if (user == null)
                    return NotFound(new { success = false, message = "کاربر یافت نشد" });

                // حذف رمز فعلی
                var token = await _userManager.GeneratePasswordResetTokenAsync(user);
                var result = await _userManager.ResetPasswordAsync(user, token, dto.NewPassword);

                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ریست رمز عبور",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // غیرفعال کردن نیاز به تغییر رمز در لاگین بعدی (اختیاری)
                // user.RequirePasswordChange = false;
                // await _userManager.UpdateAsync(user);

                return Ok(new
                {
                    success = true,
                    message = "رمز عبور با موفقیت ریست شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ریست رمز عبور", error = ex.Message });
            }
        }

        // DTO
        public class ResetPasswordDto
        {
            public string NewPassword { get; set; } = string.Empty;
        }
        */
    }
}