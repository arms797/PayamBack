using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.Admin;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class AdminController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public AdminController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // 1️⃣ دریافت لیست ادمین‌ها
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var admins = await _context.MoshakhasatAdmins
                    .Select(a => new AdminListDto
                    {
                        Id = a.Id,
                        CodeMelli = a.CodeMelli ?? "",
                        Naam = a.Naam ?? "",
                        NaameKhanevadeghi = a.NaameKhanevadeghi ?? "",
                        Mobile = a.Mobile ?? "",
                        Email = a.Email ?? ""
                    })
                    .ToListAsync();

                return Ok(new { success = true, message = "لیست ادمین‌ها دریافت شد", data = admins });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت ادمین‌ها", error = ex.Message });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک ادمین
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var admin = await _context.MoshakhasatAdmins
                    .FirstOrDefaultAsync(a => a.Id == id);

                if (admin == null)
                    return NotFound(new { success = false, message = "ادمین یافت نشد" });

                var dto = new AdminDetailDto
                {
                    Id = admin.Id,
                    CodeMelli = admin.CodeMelli ?? "",
                    Naam = admin.Naam ?? "",
                    NaameKhanevadeghi = admin.NaameKhanevadeghi ?? "",
                    TelefonMostaghim = admin.TelefonMostaghim ?? "",
                    TelefonGhayreMostaghim = admin.TelefonGhayreMostaghim ?? "",
                    TelefonDakheli = admin.TelefonDakheli ?? "",
                    Mobile = admin.Mobile ?? "",
                    Mobile2 = admin.Mobile2 ?? "",
                    Email = admin.Email ?? "",
                    Adres = admin.Adres ?? "",
                    CodePosti = admin.CodePosti ?? ""
                };

                return Ok(new { success = true, message = "اطلاعات ادمین دریافت شد", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت ادمین", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد ادمین جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] AdminCreateDto dto)
        {
            try
            {
                // بررسی کد ملی تکراری
                var exists = await _context.MoshakhasatAdmins
                    .AnyAsync(a => a.CodeMelli == dto.CodeMelli);

                if (exists)
                    return BadRequest(new { success = false, message = "کد ملی قبلاً ثبت شده است" });

                // بررسی تکراری بودن نام کاربری
                var existingUser = await _userManager.FindByNameAsync(dto.UserName);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "نام کاربری قبلاً ثبت شده است" });

                // ============================================================
                // 1️⃣ ایجاد ادمین
                // ============================================================
                var admin = new MoshakhasatAdmin
                {
                    CodeMelli = dto.CodeMelli,
                    Naam = dto.Naam,
                    NaameKhanevadeghi = dto.NaameKhanevadeghi,
                    TelefonMostaghim = dto.TelefonMostaghim,
                    TelefonGhayreMostaghim = dto.TelefonGhayreMostaghim,
                    TelefonDakheli = dto.TelefonDakheli,
                    Mobile = dto.Mobile,
                    Mobile2 = dto.Mobile2,
                    Email = dto.Email,
                    Adres = dto.Adres,
                    CodePosti = dto.CodePosti
                };

                await _context.MoshakhasatAdmins.AddAsync(admin);
                await _context.SaveChangesAsync();

                // ============================================================
                // 2️⃣ ایجاد کاربر متناظر
                // ============================================================
                var user = new AppUser
                {
                    UserName = dto.UserName,
                    Email = dto.Email,
                    AdminId = admin.Id,
                    Vazeeyat = true,
                    VazeeyatMovaghat = false
                };

                var password = dto.CodeMelli + "aA"; // رمز = کد ملی
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _context.MoshakhasatAdmins.Remove(admin);
                    await _context.SaveChangesAsync();

                    return BadRequest(new
                    {
                        success = false,
                        message = "خطا در ایجاد کاربر",
                        errors = result.Errors.Select(e => e.Description)
                    });
                }

                // اضافه کردن نقش
                if (!string.IsNullOrEmpty(dto.RoleName))
                {
                    await _userManager.AddToRoleAsync(user, dto.RoleName);
                }

                return Ok(new
                {
                    success = true,
                    message = "ادمین و کاربر با موفقیت ایجاد شد",
                    data = new { adminId = admin.Id, userId = user.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد ادمین", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ ویرایش ادمین
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] AdminUpdateDto dto)
        {
            try
            {
                var admin = await _context.MoshakhasatAdmins.FindAsync(id);
                if (admin == null)
                    return NotFound(new { success = false, message = "ادمین یافت نشد" });

                admin.Naam = dto.Naam ?? admin.Naam;
                admin.NaameKhanevadeghi = dto.NaameKhanevadeghi ?? admin.NaameKhanevadeghi;
                admin.TelefonMostaghim = dto.TelefonMostaghim ?? admin.TelefonMostaghim;
                admin.TelefonGhayreMostaghim = dto.TelefonGhayreMostaghim ?? admin.TelefonGhayreMostaghim;
                admin.TelefonDakheli = dto.TelefonDakheli ?? admin.TelefonDakheli;
                admin.Mobile = dto.Mobile ?? admin.Mobile;
                admin.Mobile2 = dto.Mobile2 ?? admin.Mobile2;
                admin.Email = dto.Email ?? admin.Email;
                admin.Adres = dto.Adres ?? admin.Adres;
                admin.CodePosti = dto.CodePosti ?? admin.CodePosti;

                await _context.SaveChangesAsync();

                // به‌روزرسانی ایمیل کاربر
                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.AdminId == id);

                if (user != null && !string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { success = true, message = "ادمین ویرایش شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش ادمین", error = ex.Message });
            }
        }

        // ============================================================
        // 5️⃣ حذف ادمین
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var admin = await _context.MoshakhasatAdmins.FindAsync(id);
                if (admin == null)
                    return NotFound(new { success = false, message = "ادمین یافت نشد" });

                // جلوگیری از حذف ادمین اصلی
                if (admin.Email == "admin@payam.ac.ir")
                    return BadRequest(new { success = false, message = "ادمین اصلی قابل حذف نیست" });

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.AdminId == id);

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.MoshakhasatAdmins.Remove(admin);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "ادمین و کاربر مربوطه حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف ادمین", error = ex.Message });
            }
        }
    }
}