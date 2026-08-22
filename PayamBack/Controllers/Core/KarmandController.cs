using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Karmand;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;
using PayamBack.Services.Interfaces;
using System.Security.Claims;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class KarmandController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;

        public KarmandController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ICurrentUserService currentUserService,
            IAccessService accessService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _accessService = accessService;
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
                // 🔥 دریافت اطلاعات کاربر فعلی از سرویس کش
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 🔥 دریافت مراکز قابل دسترس از سرویس Access
                // ============================================================
                var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);

                if (!accessibleMarkazIds.Any())
                    return Ok(new { success = true, message = "شما دسترسی به هیچ مرکزی ندارید", data = new List<object>(), pagination = new { page, pageSize, totalCount = 0, totalPages = 0 } });

                // ============================================================
                // 1️⃣ ساخت کوئری پایه با Join به AppUser
                // ============================================================
                var query = from k in _context.Karmands
                            join u in _context.Users on k.Id equals u.KarmandId into userJoin
                            from u in userJoin.DefaultIfEmpty()
                            where k.MarkazId.HasValue
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
                if (vazeeat == 1)
                {
                    query = query.Where(x => x.User != null &&
                        (x.User.Vazeeyat == true && x.User.VazeeyatMovaghat == true));
                }
                else if (vazeeat == 2)
                {
                    query = query.Where(x => x.User != null &&
                        (x.User.Vazeeyat == false || x.User.VazeeyatMovaghat == false));
                }
                else
                {
                    query = query.Where(x => x.User != null);
                }

                // ============================================================
                // 5️⃣ محاسبه تعداد کل و صفحه‌بندی
                // ============================================================
                var totalCount = await query.CountAsync();

                var karmands = await query
                    .OrderBy(x => x.Karmand.NaameKhanevadeghi)
                    .ThenBy(x => x.Karmand.Naam)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(x => new KarmandListDto
                    {
                        Id = x.Karmand.Id,
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
                // 🔥 دریافت اطلاعات کاربر فعلی از سرویس کش
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 🔥 بررسی دسترسی به مرکز هدف با IAccessService
                // ============================================================
                if (!await _accessService.CanAccessTargetMarkazAsync(dto.MarkazId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

                // ============================================================
                // 1️⃣ اعتبارسنجی ورودی
                // ============================================================
                var exists = await _context.Karmands.AnyAsync(k => k.CodeMelli == dto.CodeMelli);
                if (exists)
                    return BadRequest(new { success = false, message = "کد ملی قبلاً ثبت شده است" });

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

                var password = dto.CodeMelli + "aA";
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
                // 🔥 دریافت اطلاعات کاربر فعلی از سرویس کش
                // ============================================================
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                var karmand = await _context.Karmands
                    .Include(k => k.Markaz)
                    .FirstOrDefaultAsync(k => k.Id == id);

                if (karmand == null)
                    return NotFound(new { success = false, message = "کارمند یافت نشد" });

                // ============================================================
                // 🔥 بررسی دسترسی به مرکز هدف با IAccessService
                // ============================================================
                if (!await _accessService.CanAccessTargetMarkazAsync(karmand.MarkazId ?? 0, codeRole.Value, currentMarkaz?.Id))
                    return Forbid();

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
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
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
        // 6️⃣ تغییر وضعیت گروهی کاربران (فقط ادمین سامانه)
        // ============================================================
        [HttpPatch("toggle")]
        public async Task<IActionResult> Toggle([FromBody] List<ToggleUserStatusItemDto> items)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                if (codeRole != 1)
                    return Forbid();

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

                // دریافت کاربران هدف
                var userIds = items.Select(x => x.UserId).Distinct().ToList();

                var targetUsers = await _userManager.Users
                    .Include(u => u.Karmand)
                        .ThenInclude(k => k.Markaz)
                    .Where(u => userIds.Contains(u.Id))
                    .ToListAsync();

                if (!targetUsers.Any())
                    return NotFound(new { success = false, message = "هیچ کاربری یافت نشد" });

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

                var updatedUsers = new List<object>();
                var failedUsers = new List<object>();

                foreach (var (item, user) in validItems)
                {
                    try
                    {
                        if (item.Vazeeyat.HasValue)
                            user.Vazeeyat = item.Vazeeyat.Value;

                        if (item.VazeeyatMovaghat.HasValue)
                            user.VazeeyatMovaghat = item.VazeeyatMovaghat.Value;

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

                return Ok(new
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
                });
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
    }
}