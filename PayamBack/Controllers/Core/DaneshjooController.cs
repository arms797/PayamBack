using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using OfficeOpenXml;
using PayamBack.Data;
using PayamBack.DTOs.Core.Daneshjoo;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class DaneshjooController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;

        public DaneshjooController(AppDbContext context, UserManager<AppUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // ============================================================
        // 1️⃣ دریافت لیست دانشجویان
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList()
        {
            try
            {
                var daneshjoos = await _context.Daneshjoos
                    .Include(d => d.Markaz)
                    .Include(d => d.Reshteh)
                    .Select(d => new DaneshjooListDto
                    {
                        Id = d.Id,
                        ShomareDaneshjooee = d.ShomareDaneshjooee ?? "",
                        Naam = d.Naam ?? "",
                        NaamKhanevadegi = d.NaamKhanevadegi ?? "",
                        MarkazId = d.MarkazId ?? 0,
                        MarkazName = d.Markaz != null ? d.Markaz.NaamMarkaz ?? "" : "",
                        ReshtehId = d.ReshtehId ?? 0,
                        ReshtehName = d.Reshteh != null ? d.Reshteh.OnvanReshte ?? "" : "",
                        Mobile = d.Mobile ?? "",
                        Email = d.Email ?? ""
                    })
                    .ToListAsync();

                return Ok(new { success = true, message = "لیست دانشجویان دریافت شد", data = daneshjoos });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت دانشجویان", error = ex.Message });
            }
        }

        // ============================================================
        // 2️⃣ دریافت یک دانشجو
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var daneshjoo = await _context.Daneshjoos
                    .Include(d => d.Markaz)
                    .Include(d => d.MarkazAzmoon)
                    .Include(d => d.MarkazTermi)
                    .Include(d => d.Reshteh)
                    .FirstOrDefaultAsync(d => d.Id == id);

                if (daneshjoo == null)
                    return NotFound(new { success = false, message = "دانشجو یافت نشد" });

                var dto = new DaneshjooDetailDto
                {
                    Id = daneshjoo.Id,
                    ShomareDaneshjooee = daneshjoo.ShomareDaneshjooee ?? "",
                    Naam = daneshjoo.Naam ?? "",
                    NaamKhanevadegi = daneshjoo.NaamKhanevadegi ?? "",
                    MarkazId = daneshjoo.MarkazId ?? 0,
                    MarkazName = daneshjoo.Markaz?.NaamMarkaz ?? "",
                    MarkazAzmoonId = daneshjoo.MarkazAzmoonId ?? 0,
                    MarkazAzmoonName = daneshjoo.MarkazAzmoon?.NaamMarkaz ?? "",
                    MarkazTermiId = daneshjoo.MarkazTermiId ?? 0,
                    MarkazTermiName = daneshjoo.MarkazTermi?.NaamMarkaz ?? "",
                    ReshtehId = daneshjoo.ReshtehId ?? 0,
                    ReshtehName = daneshjoo.Reshteh?.OnvanReshte ?? "",
                    Jens = daneshjoo.Jens ?? "",
                    Naampedar = daneshjoo.Naampedar ?? "",
                    ShomareMelli = daneshjoo.ShomareMelli ?? "",
                    ShomareShenasname = daneshjoo.ShomareShenasname ?? "",
                    TarikhTavalod = daneshjoo.TarikhTavalod,
                    TermVorood = daneshjoo.TermVorood ?? "",
                    Mobile = daneshjoo.Mobile ?? "",
                    Email = daneshjoo.Email ?? ""
                };

                return Ok(new { success = true, message = "اطلاعات دانشجو دریافت شد", data = dto });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت دانشجو", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ ایجاد دانشجو جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] DaneshjooCreateDto dto)
        {
            try
            {
                // بررسی شماره دانشجویی تکراری
                var exists = await _context.Daneshjoos
                    .AnyAsync(d => d.ShomareDaneshjooee == dto.ShomareDaneshjooee);

                if (exists)
                    return BadRequest(new { success = false, message = "شماره دانشجویی قبلاً ثبت شده است" });

                // بررسی تکراری بودن نام کاربری
                var existingUser = await _userManager.FindByNameAsync(dto.ShomareDaneshjooee);
                if (existingUser != null)
                    return BadRequest(new { success = false, message = "شماره دانشجویی قبلاً به عنوان نام کاربری ثبت شده است" });

                // ============================================================
                // 1️⃣ ایجاد دانشجو
                // ============================================================
                var daneshjoo = new Daneshjoo
                {
                    ShomareDaneshjooee = dto.ShomareDaneshjooee,
                    Naam = dto.Naam,
                    NaamKhanevadegi = dto.NaamKhanevadegi,
                    MarkazId = dto.MarkazId,
                    MarkazAzmoonId = dto.MarkazAzmoonId,
                    MarkazTermiId = dto.MarkazTermiId,
                    ReshtehId = dto.ReshtehId,
                    Jens = dto.Jens,
                    Naampedar = dto.Naampedar,
                    ShomareMelli = dto.ShomareMelli,
                    ShomareShenasname = dto.ShomareShenasname,
                    TarikhTavalod = dto.TarikhTavalod,
                    TermVorood = dto.TermVorood,
                    Mobile = dto.Mobile,
                    Email = dto.Email
                };

                await _context.Daneshjoos.AddAsync(daneshjoo);
                await _context.SaveChangesAsync();

                // ============================================================
                // 2️⃣ ایجاد کاربر متناظر (نام کاربری = شماره دانشجویی)
                // ============================================================
                var user = new AppUser
                {
                    UserName = dto.ShomareDaneshjooee,
                    Email = dto.Email,
                    DaneshjooId = daneshjoo.Id,
                    Vazeeyat = true,
                    VazeeyatMovaghat = false
                };

                var password = dto.ShomareMelli; // رمز = کد ملی
                var result = await _userManager.CreateAsync(user, password);

                if (!result.Succeeded)
                {
                    _context.Daneshjoos.Remove(daneshjoo);
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
                    message = "دانشجو و کاربر با موفقیت ایجاد شد",
                    data = new { daneshjooId = daneshjoo.Id, userId = user.Id }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد دانشجو", error = ex.Message });
            }
        }

        // ============================================================
        // 4️⃣ آپلود گروهی دانشجویان از Excel
        // ============================================================
        [HttpPost("bulk-upload")]
        public async Task<IActionResult> BulkUpload(IFormFile file)
        {
            try
            {
                if (file == null || file.Length == 0)
                    return BadRequest(new { success = false, message = "فایل انتخاب نشده است" });

                if (!file.FileName.EndsWith(".xlsx"))
                    return BadRequest(new { success = false, message = "فرمت فایل باید xlsx باشد" });

                var daneshjoos = new List<Daneshjoo>();
                var users = new List<AppUser>();
                var errors = new List<string>();
                var batchSize = 200;
                var rowCount = 0;

                using var stream = new MemoryStream();
                await file.CopyToAsync(stream);
                using var package = new ExcelPackage(stream);
                var worksheet = package.Workbook.Worksheets[0];
                var rowCountTotal = worksheet.Dimension?.Rows ?? 0;

                if (rowCountTotal < 2)
                    return BadRequest(new { success = false, message = "فایل خالی است" });

                for (int row = 2; row <= rowCountTotal; row++)
                {
                    try
                    {
                        var shomareDaneshjooee = worksheet.Cells[row, 1].Text?.Trim();
                        var naam = worksheet.Cells[row, 2].Text?.Trim();
                        var naamKhanevadegi = worksheet.Cells[row, 3].Text?.Trim();
                        var shomareMelli = worksheet.Cells[row, 4].Text?.Trim();
                        var email = worksheet.Cells[row, 5].Text?.Trim();
                        var mobile = worksheet.Cells[row, 6].Text?.Trim();
                        var markazId = int.TryParse(worksheet.Cells[row, 7].Text?.Trim(), out int mId) ? mId : (int?)null;
                        var reshtehId = int.TryParse(worksheet.Cells[row, 8].Text?.Trim(), out int rId) ? rId : (int?)null;

                        if (string.IsNullOrEmpty(shomareDaneshjooee) || string.IsNullOrEmpty(shomareMelli))
                        {
                            errors.Add($"ردیف {row}: شماره دانشجویی و کد ملی الزامی است");
                            continue;
                        }

                        // بررسی تکراری در لیست فعلی
                        if (daneshjoos.Any(d => d.ShomareDaneshjooee == shomareDaneshjooee))
                        {
                            errors.Add($"ردیف {row}: شماره دانشجویی {shomareDaneshjooee} تکراری است");
                            continue;
                        }

                        // بررسی تکراری در دیتابیس
                        var exists = await _context.Daneshjoos.AnyAsync(d => d.ShomareDaneshjooee == shomareDaneshjooee);
                        if (exists)
                        {
                            errors.Add($"ردیف {row}: شماره دانشجویی {shomareDaneshjooee} قبلاً ثبت شده است");
                            continue;
                        }

                        var userExists = await _userManager.FindByNameAsync(shomareDaneshjooee);
                        if (userExists != null)
                        {
                            errors.Add($"ردیف {row}: شماره دانشجویی {shomareDaneshjooee} قبلاً به عنوان نام کاربری ثبت شده است");
                            continue;
                        }

                        var daneshjoo = new Daneshjoo
                        {
                            ShomareDaneshjooee = shomareDaneshjooee,
                            Naam = naam,
                            NaamKhanevadegi = naamKhanevadegi,
                            ShomareMelli = shomareMelli,
                            Email = email,
                            Mobile = mobile,
                            MarkazId = markazId,
                            ReshtehId = reshtehId
                        };

                        daneshjoos.Add(daneshjoo);

                        var user = new AppUser
                        {
                            UserName = shomareDaneshjooee,
                            Email = email,
                            DaneshjooId = daneshjoo.Id,
                            Vazeeyat = true,
                            VazeeyatMovaghat = false
                        };
                        users.Add(user);

                        rowCount++;

                        if (rowCount % batchSize == 0)
                        {
                            await SaveDaneshjooBatch(daneshjoos, users);
                            daneshjoos.Clear();
                            users.Clear();
                        }
                    }
                    catch (Exception ex)
                    {
                        errors.Add($"ردیف {row}: خطا در پردازش - {ex.Message}");
                    }
                }

                if (daneshjoos.Any())
                {
                    await SaveDaneshjooBatch(daneshjoos, users);
                }

                return Ok(new
                {
                    success = true,
                    message = $"تعداد {rowCount} دانشجو با موفقیت ثبت شد",
                    errors = errors.Any() ? errors : null,
                    errorCount = errors.Count
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در آپلود فایل", error = ex.Message });
            }
        }

        private async Task SaveDaneshjooBatch(List<Daneshjoo> daneshjoos, List<AppUser> users)
        {
            await _context.Daneshjoos.AddRangeAsync(daneshjoos);
            await _context.SaveChangesAsync();

            foreach (var user in users)
            {
                var password = daneshjoos.First(d => d.Id == user.DaneshjooId)?.ShomareMelli ?? "123456";
                await _userManager.CreateAsync(user, password);
            }
        }

        // ============================================================
        // 5️⃣ ویرایش دانشجو
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] DaneshjooUpdateDto dto)
        {
            try
            {
                var daneshjoo = await _context.Daneshjoos.FindAsync(id);
                if (daneshjoo == null)
                    return NotFound(new { success = false, message = "دانشجو یافت نشد" });

                daneshjoo.Naam = dto.Naam ?? daneshjoo.Naam;
                daneshjoo.NaamKhanevadegi = dto.NaamKhanevadegi ?? daneshjoo.NaamKhanevadegi;
                daneshjoo.MarkazId = dto.MarkazId ?? daneshjoo.MarkazId;
                daneshjoo.ReshtehId = dto.ReshtehId ?? daneshjoo.ReshtehId;
                daneshjoo.Mobile = dto.Mobile ?? daneshjoo.Mobile;
                daneshjoo.Email = dto.Email ?? daneshjoo.Email;

                await _context.SaveChangesAsync();

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DaneshjooId == id);

                if (user != null && !string.IsNullOrEmpty(dto.Email))
                {
                    user.Email = dto.Email;
                    await _userManager.UpdateAsync(user);
                }

                return Ok(new { success = true, message = "دانشجو ویرایش شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ویرایش دانشجو", error = ex.Message });
            }
        }

        // ============================================================
        // 6️⃣ حذف دانشجو
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var daneshjoo = await _context.Daneshjoos.FindAsync(id);
                if (daneshjoo == null)
                    return NotFound(new { success = false, message = "دانشجو یافت نشد" });

                var user = await _userManager.Users
                    .FirstOrDefaultAsync(u => u.DaneshjooId == id);

                if (user != null)
                {
                    await _userManager.DeleteAsync(user);
                }

                _context.Daneshjoos.Remove(daneshjoo);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "دانشجو و کاربر مربوطه حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف دانشجو", error = ex.Message });
            }
        }
    }
}