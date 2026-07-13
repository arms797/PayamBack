using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Identity.Menu;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Identity
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "ادمین سامانه")]
    public class MenuController : ControllerBase
    {
        private readonly AppDbContext _context;

        public MenuController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت لیست کامل منوها (به صورت درختی)
        // ============================================================
        [HttpGet("tree")]
        public async Task<IActionResult> GetTree()
        {
            try
            {
                var allMenus = await _context.Menus
                    .OrderBy(m => m.Order)
                    .ToListAsync();

                var tree = BuildMenuTree(allMenus, null);

                return Ok(new
                {
                    success = true,
                    message = "ساختار منوها با موفقیت دریافت شد",
                    data = tree
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت ساختار منوها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ دریافت لیست ساده منوها (برای انتخاب والد)
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var menus = await _context.Menus
                    .OrderBy(m => m.Order)
                    .Select(m => new MenuListDto
                    {
                        Id = m.Id,
                        Title = m.Title ?? "",
                        ParentId = m.ParentId,
                        Path = m.Path ?? "",
                        Icon = m.Icon ?? "",
                        PermissionName = m.PermissionName ?? "",
                        Order = m.Order ?? 0,
                        Vazeeat = m.Vazeeat ?? false
                    })
                    .ToListAsync();

                return Ok(new
                {
                    success = true,
                    message = "لیست منوها با موفقیت دریافت شد",
                    data = menus
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت لیست منوها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ دریافت یک منو
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var menu = await _context.Menus
                    .Where(m => m.Id == id)
                    .Select(m => new MenuDetailDto
                    {
                        Id = m.Id,
                        Title = m.Title ?? "",
                        ParentId = m.ParentId,
                        Path = m.Path ?? "",
                        Icon = m.Icon ?? "",
                        PermissionName = m.PermissionName ?? "",
                        Order = m.Order ?? 0,
                        Vazeeat = m.Vazeeat ?? false,
                        CreatedAt = m.CreatedAt,
                        UpdatedAt = m.UpdatedAt
                    })
                    .FirstOrDefaultAsync();

                if (menu == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "منو مورد نظر یافت نشد"
                    });
                }

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات منو با موفقیت دریافت شد",
                    data = menu
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات منو",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ ایجاد منوی جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] MenuCreateDto dto)
        {
            try
            {
                // اگر ParentId وارد شده، بررسی وجود آن
                if (dto.ParentId.HasValue)
                {
                    var parentExists = await _context.Menus
                        .AnyAsync(m => m.Id == dto.ParentId.Value);

                    if (!parentExists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "منوی والد مورد نظر یافت نشد"
                        });
                    }
                }

                var menu = new Menu
                {
                    Title = dto.Title,
                    ParentId = dto.ParentId,
                    Path = dto.Path,
                    Icon = dto.Icon,
                    PermissionName = dto.PermissionName,
                    Order = dto.Order ?? 0,
                    Vazeeat = dto.Vazeeat ?? true,
                    CreatedAt = DateTime.UtcNow
                };

                await _context.Menus.AddAsync(menu);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "منو با موفقیت ایجاد شد",
                    data = new { id = menu.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد منو",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ ویرایش منو
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] MenuUpdateDto dto)
        {
            try
            {
                var menu = await _context.Menus
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (menu == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "منو مورد نظر یافت نشد"
                    });
                }

                // اگر ParentId تغییر کرده، بررسی وجود آن
                if (dto.ParentId.HasValue && dto.ParentId.Value != menu.ParentId)
                {
                    // جلوگیری از حلقه (نمی‌تواند والد خودش باشد)
                    if (dto.ParentId.Value == id)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "یک منو نمی‌تواند والد خودش باشد"
                        });
                    }

                    var parentExists = await _context.Menus
                        .AnyAsync(m => m.Id == dto.ParentId.Value);

                    if (!parentExists)
                    {
                        return BadRequest(new
                        {
                            success = false,
                            message = "منوی والد مورد نظر یافت نشد"
                        });
                    }
                }

                menu.Title = dto.Title ?? menu.Title;
                menu.ParentId = dto.ParentId;
                menu.Path = dto.Path ?? menu.Path;
                menu.Icon = dto.Icon ?? menu.Icon;
                menu.PermissionName = dto.PermissionName ?? menu.PermissionName;
                menu.Order = dto.Order ?? menu.Order;
                menu.Vazeeat = dto.Vazeeat ?? menu.Vazeeat;
                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "منو با موفقیت بروزرسانی شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بروزرسانی منو",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ حذف منو (به همراه زیرمنوها)
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var menu = await _context.Menus
                    .Include(m => m.Children)
                    .FirstOrDefaultAsync(m => m.Id == id);

                if (menu == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "منو مورد نظر یافت نشد"
                    });
                }

                // بررسی وجود زیرمنوها
                if (menu.Children != null && menu.Children.Any())
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = "این منو دارای زیرمنو است. ابتدا زیرمنوها را حذف کنید"
                    });
                }

                _context.Menus.Remove(menu);
                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "منو با موفقیت حذف شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف منو",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ تغییر ترتیب منوها
        // ============================================================
        [HttpPut("reorder")]
        public async Task<IActionResult> Reorder([FromBody] MenuReorderDto dto)
        {
            try
            {
                var menu = await _context.Menus
                    .FirstOrDefaultAsync(m => m.Id == dto.Id);

                if (menu == null)
                {
                    return NotFound(new
                    {
                        success = false,
                        message = "منو مورد نظر یافت نشد"
                    });
                }

                menu.Order = dto.NewOrder;
                menu.UpdatedAt = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                return Ok(new
                {
                    success = true,
                    message = "ترتیب منو با موفقیت تغییر کرد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تغییر ترتیب منو",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 8️⃣ دریافت منوهای فعال (برای فرانت‌اند)
        // ============================================================
        [HttpGet("active")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveMenus()
        {
            try
            {
                var allMenus = await _context.Menus
                    .Where(m => m.Vazeeat == true)
                    .OrderBy(m => m.Order)
                    .ToListAsync();

                var tree = BuildMenuTree(allMenus, null);

                return Ok(new
                {
                    success = true,
                    message = "منوهای فعال با موفقیت دریافت شد",
                    data = tree
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت منوهای فعال",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // متد کمکی: ساخت درخت منو
        // ============================================================
        private List<MenuTreeDto> BuildMenuTree(List<Menu> allMenus, int? parentId)
        {
            return allMenus
                .Where(m => m.ParentId == parentId)
                .OrderBy(m => m.Order)
                .Select(m => new MenuTreeDto
                {
                    Id = m.Id,
                    Title = m.Title ?? "",
                    ParentId = m.ParentId,
                    Path = m.Path ?? "",
                    Icon = m.Icon ?? "",
                    PermissionName = m.PermissionName ?? "",
                    Order = m.Order ?? 0,
                    Vazeeat = m.Vazeeat ?? false,
                    Children = BuildMenuTree(allMenus, m.Id)
                })
                .ToList();
        }
    }
}