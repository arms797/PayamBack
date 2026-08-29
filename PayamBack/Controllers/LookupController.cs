using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PayamBack.Services.Interfaces;

namespace PayamBack.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous] // یا [Authorize] بسته به نیاز
    public class LookupController : ControllerBase
    {
        private readonly ILookupCacheService _lookupCache;

        public LookupController(ILookupCacheService lookupCache)
        {
            _lookupCache = lookupCache;
        }

        /// <summary>
        /// دریافت تمام داده‌های مرجع (روزها، ساعت‌ها، فعالیت‌ها) با کش
        /// </summary>
        [HttpGet("metadata")]
        public async Task<IActionResult> GetMetadata()
        {
            try
            {
                var data = await _lookupCache.GetAllAsync();
                return Ok(new
                {
                    success = true,
                    message = "داده‌های مرجع با موفقیت دریافت شد",
                    data = new
                    {
                        days = data.Days,//.Select(d => new { d.Id, d.Code, d.Title, d.IsActive, d.Order, d.IsHoliday }),
                        hours = data.Hours,//.Select(h => new { h.Id, h.CodeSaat, h.OnvanSaat, h.SaatShoroo, h.SaatPayan, h.Hozoori, h.Majazi }),
                        faaliats = data.Faaliats//.Select(a => new { a.Id, a.Onvan, a.NoeAnjam, a.Color, a.MinSaatDarHafteh, a.MaxSaatDarHafteh, a.IsMadove })
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت داده‌های مرجع",
                    error = ex.Message
                });
            }
        }

        /// <summary>
        /// پاک کردن کش داده‌های مرجع (فقط ادمین)
        /// </summary>
        [HttpDelete("clear-cache")]
        [Authorize(Roles = "ادمین سامانه")]
        public IActionResult ClearCache()
        {
            _lookupCache.ClearCache();
            return Ok(new { success = true, message = "کش داده‌های مرجع پاک شد" });
        }
    }
}