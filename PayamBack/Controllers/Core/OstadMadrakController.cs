using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Core.OstadMadrak;
using PayamBack.Models.Core;

namespace PayamBack.Controllers.Core
{
    [ApiController]
    [Route("api/[controller]")]
    public class OstadMadrakController : ControllerBase
    {
        private readonly AppDbContext _context;

        public OstadMadrakController(AppDbContext context)
        {
            _context = context;
        }

        // ============================================================
        // 1️⃣ دریافت مدارک یک استاد
        // ============================================================
        [HttpGet("by-ostad/{ostadId}")]
        public async Task<IActionResult> GetByOstadId(int ostadId)
        {
            try
            {
                var madraks = await _context.OstadMadraks
                    .Include(m => m.GrooheAmoozeshi)
                    .Where(m => m.OstadId == ostadId)
                    .Select(m => new OstadMadrakListDto
                    {
                        Id = m.Id,
                        OstadId = m.OstadId ?? 0,
                        Reshteh = m.Reshteh ?? "",
                        Grayesh = m.Grayesh ?? "",
                        Maghta = m.Maghta ?? 0,
                        PishFarz = m.PishFarz ?? false,
                        MahalAkhz = m.MahalAkhz ?? "",
                        TasvirMadrak = m.TasvirMadrak ?? "",
                        GrooheAmoozeshiId = m.GrooheAmoozeshiId ?? 0,
                        GrooheAmoozeshiName = m.GrooheAmoozeshi != null ? m.GrooheAmoozeshi.OnvanGrooheAmoozeshi ?? "" : ""
                    })
                    .ToListAsync();

                return Ok(new { success = true, message = "مدارک استاد دریافت شد", data = madraks });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در دریافت مدارک", error = ex.Message });
            }
        }

        // ============================================================
        // 2️⃣ ایجاد مدرک جدید
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] OstadMadrakCreateDto dto)
        {
            try
            {
                var madrak = new OstadMadrak
                {
                    OstadId = dto.OstadId,
                    Reshteh = dto.Reshteh,
                    Grayesh = dto.Grayesh,
                    Maghta = dto.Maghta,
                    PishFarz = dto.PishFarz ?? false,
                    MahalAkhz = dto.MahalAkhz,
                    TasvirMadrak = dto.TasvirMadrak,
                    GrooheAmoozeshiId = dto.GrooheAmoozeshiId
                };

                if (madrak.PishFarz == true)
                {
                    await _context.OstadMadraks
                        .Where(m => m.OstadId == dto.OstadId && m.PishFarz == true)
                        .ForEachAsync(m => m.PishFarz = false);
                }

                await _context.OstadMadraks.AddAsync(madrak);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "مدرک ایجاد شد", data = new { id = madrak.Id } });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در ایجاد مدرک", error = ex.Message });
            }
        }

        // ============================================================
        // 3️⃣ حذف مدرک
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var madrak = await _context.OstadMadraks.FindAsync(id);
                if (madrak == null)
                    return NotFound(new { success = false, message = "مدرک یافت نشد" });

                _context.OstadMadraks.Remove(madrak);
                await _context.SaveChangesAsync();

                return Ok(new { success = true, message = "مدرک حذف شد" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, message = "خطا در حذف مدرک", error = ex.Message });
            }
        }
    }
}