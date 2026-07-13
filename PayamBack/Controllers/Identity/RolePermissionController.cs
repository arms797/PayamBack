using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity.RolePermission;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ادمین سامانه")]
    public class RolePermissionController : ControllerBase
    {
        private readonly AppDbContext _context;

        public RolePermissionController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه مجوزهای اختصاص‌یافته به نقش‌ها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var rolePermissions = await _context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .Select(rp => new RolePermissionListDto
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId ?? 0,
                        RoleName = rp.Role != null ? rp.Role.Name ?? "" : "",
                        PermissionId = rp.PermissionId ?? 0,
                        PermissionName = rp.Permission != null ? rp.Permission.Name ?? "" : "",
                        Vazeeat = rp.Vazeeat ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مجوزهای نقش‌ها با موفقیت دریافت شد",
                    data = rolePermissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست مجوزهای نقش‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت مجوزهای یک نقش خاص
        // ============================================================
        [HttpGet("by-role/{roleId}")]
        public async Task<IActionResult> GetByRoleId(int roleId)
        {
            try
            {
                // بررسی وجود نقش
                var roleExists = await _context.Roles
                    .AnyAsync(r => r.Id == roleId);

                if (!roleExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نقش مورد نظر یافت نشد"
                    });
                }

                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.RoleId == roleId)
                    .Include(rp => rp.Permission)
                    .Select(rp => new RolePermissionDetailDto
                    {
                        Id = rp.Id,
                        PermissionId = rp.PermissionId ?? 0,
                        PermissionName = rp.Permission != null ? rp.Permission.Name ?? "" : "",
                        Resource = rp.Permission != null ? rp.Permission.Resource ?? "" : "",
                        Action = rp.Permission != null ? rp.Permission.Action ?? "" : "",
                        Vazeeat = rp.Vazeeat ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست مجوزهای نقش با موفقیت دریافت شد",
                    data = rolePermissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت مجوزهای نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ دریافت نقش‌های یک مجوز خاص
        // ============================================================
        [HttpGet("by-permission/{permissionId}")]
        public async Task<IActionResult> GetByPermissionId(int permissionId)
        {
            try
            {
                var permissionExists = await _context.Permissions
                    .AnyAsync(p => p.Id == permissionId);

                if (!permissionExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مجوز مورد نظر یافت نشد"
                    });
                }

                var rolePermissions = await _context.RolePermissions
                    .Where(rp => rp.PermissionId == permissionId)
                    .Include(rp => rp.Role)
                    .Select(rp => new RolePermissionByPermissionDto
                    {
                        Id = rp.Id,
                        RoleId = rp.RoleId ?? 0,
                        RoleName = rp.Role != null ? rp.Role.Name ?? "" : "",
                        Vazeeat = rp.Vazeeat ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست نقش‌های این مجوز با موفقیت دریافت شد",
                    data = rolePermissions
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت نقش‌های مجوز",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ تخصیص مجوز به نقش
        // ============================================================
        [HttpPost("assign")]
        public async Task<IActionResult> Assign([FromBody] RolePermissionAssignDto dto)
        {
            try
            {
                // بررسی وجود نقش
                var roleExists = await _context.Roles
                    .AnyAsync(r => r.Id == dto.RoleId);

                if (!roleExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نقش مورد نظر یافت نشد"
                    });
                }

                // بررسی وجود مجوز
                var permissionExists = await _context.Permissions
                    .AnyAsync(p => p.Id == dto.PermissionId);

                if (!permissionExists)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "مجوز مورد نظر یافت نشد"
                    });
                }

                // بررسی تکراری نبودن
                var alreadyAssigned = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId);

                if (alreadyAssigned)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "این مجوز قبلاً به این نقش اختصاص داده شده است"
                    });
                }

                var rolePermission = new RolePermission
                {
                    RoleId = dto.RoleId,
                    PermissionId = dto.PermissionId,
                    Vazeeat = dto.Vazeeat ?? true
                };

                await _context.RolePermissions.AddAsync(rolePermission);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مجوز با موفقیت به نقش اختصاص داده شد",
                    data = new
                    {
                        id = rolePermission.Id,
                        roleId = dto.RoleId,
                        permissionId = dto.PermissionId,
                        vazeeat = rolePermission.Vazeeat
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تخصیص مجوز به نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف مجوز از نقش
        // ============================================================
        [HttpDelete("remove")]
        public async Task<IActionResult> Remove([FromBody] RolePermissionRemoveDto dto)
        {
            try
            {
                var rolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId);

                if (rolePermission == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "این مجوز به این نقش اختصاص داده نشده است"
                    });
                }

                _context.RolePermissions.Remove(rolePermission);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "مجوز با موفقیت از نقش حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف مجوز از نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ تغییر وضعیت مجوز در نقش (فعال/غیرفعال)
        // ============================================================
        [HttpPut("toggle")]
        public async Task<IActionResult> Toggle([FromBody] RolePermissionToggleDto dto)
        {
            try
            {
                var rolePermission = await _context.RolePermissions
                    .FirstOrDefaultAsync(rp => rp.RoleId == dto.RoleId && rp.PermissionId == dto.PermissionId);

                if (rolePermission == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "این مجوز به این نقش اختصاص داده نشده است"
                    });
                }

                rolePermission.Vazeeat = dto.Vazeeat;
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "وضعیت مجوز در نقش با موفقیت تغییر کرد",
                    data = new
                    {
                        roleId = dto.RoleId,
                        permissionId = dto.PermissionId,
                        vazeeat = rolePermission.Vazeeat
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر وضعیت مجوز در نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ دریافت یک رکورد RolePermission با شناسه
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var rolePermission = await _context.RolePermissions
                    .Include(rp => rp.Role)
                    .Include(rp => rp.Permission)
                    .Where(rp => rp.Id == id)
                    .Select(rp => new RolePermissionDetailDto
                    {
                        Id = rp.Id,
                        PermissionId = rp.PermissionId ?? 0,
                        PermissionName = rp.Permission != null ? rp.Permission.Name ?? "" : "",
                        Resource = rp.Permission != null ? rp.Permission.Resource ?? "" : "",
                        Action = rp.Permission != null ? rp.Permission.Action ?? "" : "",
                        Vazeeat = rp.Vazeeat ?? false
                    })
                    .FirstOrDefaultAsync();

                if (rolePermission == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "رکورد مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات با موفقیت دریافت شد",
                    data = rolePermission
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات",
                    error = ex.Message
                });
            }
        }
    }
}