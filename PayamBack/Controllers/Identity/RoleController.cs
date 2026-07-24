using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity.Role;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoleController : ControllerBase
    {
        private readonly RoleManager<AppRole> _roleManager;
        private readonly AppDbContext _context;

        public RoleController(RoleManager<AppRole> roleManager, AppDbContext context)
        {
            _roleManager = roleManager;
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست همه نقش‌ها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var roles = await _roleManager.Roles
                    .Select(r => new RoleListDto
                    {
                        Id = r.Id,
                        Name = r.Name ?? "",
                        CodeRole = r.CodeRole ?? 0,
                        Vazeeyat = r.Vazeeyat ?? true,
                        Emza = r.Emza ?? false,
                        IsAdmin = r.IsAdmin ?? false  // ← اضافه شد
                    })
                    .OrderBy(r => r.CodeRole)
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست نقش‌ها با موفقیت دریافت شد",
                    data = roles
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست نقش‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک نقش
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var role = await _roleManager.Roles
                    .Where(r => r.Id == id)
                    .Select(r => new RoleDetailDto
                    {
                        Id = r.Id,
                        Name = r.Name ?? "",
                        CodeRole = r.CodeRole ?? 0,
                        Vazeeyat = r.Vazeeyat ?? true,
                        Emza = r.Emza ?? false,
                        IsAdmin = r.IsAdmin ?? false  // ← اضافه شد
                    })
                    .FirstOrDefaultAsync();

                if (role == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نقش مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات نقش با موفقیت دریافت شد",
                    data = role
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد نقش جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] RoleCreateDto dto)
        {
            try
            {
                var exists = await _roleManager.RoleExistsAsync(dto.Name);
                if (exists)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "نقشی با این نام قبلاً ثبت شده است"
                    });
                }

                //var codeExists = await _roleManager.Roles
                //    .AnyAsync(r => r.CodeRole == dto.CodeRole);

                //if (codeExists)
                //{
                //    return BadRequest(new
                //    {
                //        success = false,
                //        message = "کد نقش تکراری است"
                //    });
                //}

                var role = new AppRole
                {
                    Name = dto.Name,
                    CodeRole = dto.CodeRole,
                    Vazeeyat = dto.Vazeeyat ?? true,
                    Emza = dto.Emza ?? false,
                    IsAdmin = dto.IsAdmin ?? false  // ← اضافه شد
                };

                var result = await _roleManager.CreateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ایجاد نقش",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "نقش با موفقیت ایجاد شد",
                    data = new { id = role.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش نقش
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] RoleUpdateDto dto)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نقش مورد نظر یافت نشد"
                    });
                }

                if (!string.IsNullOrEmpty(dto.Name) && dto.Name != role.Name)
                {
                    var exists = await _roleManager.RoleExistsAsync(dto.Name);
                    if (exists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "نقشی با این نام قبلاً ثبت شده است"
                        });
                    }
                }

                //if (dto.CodeRole.HasValue && dto.CodeRole.Value != role.CodeRole)
                //{
                //    var codeExists = await _roleManager.Roles
                //        .AnyAsync(r => r.Id != id && r.CodeRole == dto.CodeRole.Value);

                //    if (codeExists)
                //    {
                //        return BadRequest(new
                //        {
                //            success = false,
                //            message = "کد نقش تکراری است"
                //        });
                //    }
                //}

                role.Name = dto.Name ?? role.Name;
                role.CodeRole = dto.CodeRole ?? role.CodeRole;
                role.Vazeeyat = dto.Vazeeyat ?? role.Vazeeyat;
                role.Emza = dto.Emza ?? role.Emza;
                role.IsAdmin = dto.IsAdmin ?? role.IsAdmin;  // ← اضافه شد

                var result = await _roleManager.UpdateAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ویرایش نقش",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "نقش با موفقیت ویرایش شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش نقش",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ حذف نقش
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var role = await _roleManager.FindByIdAsync(id.ToString());
                if (role == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "نقش مورد نظر یافت نشد"
                    });
                }

                var isUsed = await _context.UserRoles
                    .AnyAsync(ur => ur.RoleId == id);

                if (isUsed)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "این نقش به کاربران اختصاص داده شده است و قابل حذف نیست"
                    });
                }

                var hasPermissions = await _context.RolePermissions
                    .AnyAsync(rp => rp.RoleId == id);

                if (hasPermissions)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "این نقش دارای مجوز است. ابتدا مجوزهای آن را حذف کنید"
                    });
                }

                var result = await _roleManager.DeleteAsync(role);
                if (!result.Succeeded)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در حذف نقش",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "نقش با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف نقش",
                    error = ex.Message
                });
            }
        }
    }
}