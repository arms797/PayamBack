using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Memory;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using PayamBack.Services.Implementations;
using PayamBack.Services.Interfaces;
using System.ComponentModel.DataAnnotations;

namespace PayamBack.Controllers.Schedule
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class BarnamehHaftegiController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly UserManager<AppUser> _userManager;
        private readonly RoleManager<AppRole> _roleManager;
        private readonly ICurrentUserService _currentUserService;
        private readonly IAccessService _accessService;
        private readonly IMarkazCacheService _markazCache;
        private readonly IMemoryCache _cache;
        private readonly IFaaliatCacheService _faaliatCacheService;
        private readonly ISaatBargozariCacheService _saatBargozariCacheService;

        public BarnamehHaftegiController(
            AppDbContext context,
            UserManager<AppUser> userManager,
            RoleManager<AppRole> roleManager,
            ICurrentUserService currentUserService,
            IAccessService accessService,
            IMarkazCacheService markazCache,
            IMemoryCache cache,
            IFaaliatCacheService faaliatCacheService,
            ISaatBargozariCacheService saatBargozariCacheService)
        {
            _context = context;
            _userManager = userManager;
            _roleManager = roleManager;
            _currentUserService = currentUserService;
            _accessService = accessService;
            _markazCache = markazCache;
            _cache = cache;
            _faaliatCacheService = faaliatCacheService;
            _saatBargozariCacheService = saatBargozariCacheService;
        }

        // ============================================================
        // 🔥 متدهای خصوصی کمکی
        // ============================================================

        /// <summary>
        /// دریافت ساعت موظفی استاد برای ترم مشخص
        /// اولویت: ElmiTerm (با ApproveStatus=1) → 40 ساعت پیش‌فرض
        /// </summary>
        private async Task<int> GetRequiredHoursAsync(int ostadId, string termCode)
        {
            // دریافت کاربر (برای پیدا کردن OstadId)
            var user = await _context.Users
                .Include(u=>u.Ostad)
                .FirstOrDefaultAsync(u => u.OstadId == ostadId);

            if (user == null) return 0;
            if (user.Ostad.NoeHamkari != NoeHamkariEnum.HeyatElmiPayamNoor) return 0;

            // بررسی ElmiTerm با ApproveStatus=1
            var elmiTerm = await _context.ElmiTerms
                .Where(e => e.UserId == user.Id && e.ApproveStatus == 1 && e.Vazeeat==true)
                .OrderByDescending(e => e.Id)
                .FirstOrDefaultAsync();

            if (elmiTerm?.TedadSaatMovazafi.HasValue == true && elmiTerm.TedadSaatMovazafi.Value > 0)
            {
                return elmiTerm.TedadSaatMovazafi.Value;
            }

            return 40; // پیش‌فرض
        }

        /// <summary>
        /// محاسبه تعداد جلسات پر شده در برنامه
        /// هر سلول پر شده = ۱ جلسه (معادل ۲ ساعت)
        /// </summary>
        private int CalculateTotalSessions(BarnamehHaftegiOstad program)
        {
            int count = 0;
            foreach (var detail in program.BarnamehHaftegiOstad1s)
            {
                if (detail.A.HasValue && detail.A.Value > 0) count++;
                if (detail.B.HasValue && detail.B.Value > 0) count++;
                if (detail.C.HasValue && detail.C.Value > 0) count++;
                if (detail.D.HasValue && detail.D.Value > 0) count++;
                if (detail.E.HasValue && detail.E.Value > 0) count++;
                if (detail.F.HasValue && detail.F.Value > 0) count++;
                if (detail.G.HasValue && detail.G.Value > 0) count++;
                if (detail.H.HasValue && detail.H.Value > 0) count++;
            }
            return count;
        }

        /// <summary>
        /// دریافت مراکز مجاز برای یک استاد در ترم مشخص
        /// مرکز اصلی + مراکزی که در Hamjavar1 مجوز گرفته‌اند
        /// </summary>
        private async Task<List<PermittedMarkazInfo>> GetPermittedMarkazInfoAsync(int ostadId, string termCode)
        {
            var cacheKey = $"PermittedMarkazInfo_{ostadId}_{termCode}";
            if (_cache.TryGetValue(cacheKey, out List<PermittedMarkazInfo>? cached) && cached != null)
                return cached;

            var result = new List<PermittedMarkazInfo>();

            // 1️⃣ مرکز اصلی استاد
            var ostad = await _context.Ostads
                .Include(o => o.Markaz)
                .FirstOrDefaultAsync(o => o.Id == ostadId);

            if (ostad?.MarkazId != null && ostad.Markaz != null)
            {
                result.Add(new PermittedMarkazInfo
                {
                    MarkazId = ostad.MarkazId.Value,
                    IsMainMarkaz = true,
                    MaxDays = null, // بدون محدودیت
                    AllowedFaaliatIds = new List<int>(), // همه فعالیت‌ها مجاز (بر اساس نوع مرکز)
                    NoeMarkaz = ostad.Markaz.NoeMarkaz ?? 1
                });
            }

            // 2️⃣ مراکز مجاز از Hamjavar1
            var hamjavarData = await _context.Hamjavar1s
                .Where(h => h.Hamjavar.OstadId == ostadId
                            && h.Hamjavar.TermCode == termCode
                            && h.Hamjavar.NazarMoaven == 2
                            && h.TedadRoozMoaven.HasValue
                            && h.TedadRoozMoaven.Value > 0)
                .Select(h => new
                {
                    h.MarkazId,
                    h.TedadRoozMoaven,
                    FaaliatIds = h.FaaliatIds ?? "",
                    h.Markaz.NoeMarkaz
                })
                .ToListAsync();

            foreach (var item in hamjavarData)
            {
                if (!item.MarkazId.HasValue) continue;

                var faaliatIds = string.IsNullOrEmpty(item.FaaliatIds)
                    ? new List<int>()
                    : item.FaaliatIds.Split('|', StringSplitOptions.RemoveEmptyEntries)
                        .Select(int.Parse)
                        .ToList();

                result.Add(new PermittedMarkazInfo
                {
                    MarkazId = item.MarkazId.Value,
                    IsMainMarkaz = false,
                    MaxDays = item.TedadRoozMoaven,
                    AllowedFaaliatIds = faaliatIds,
                    NoeMarkaz = item.NoeMarkaz ?? 1
                });
            }

            _cache.Set(cacheKey, result, TimeSpan.FromHours(1));
            return result;
        }

        /// <summary>
        /// بررسی حداقل ۵ روز در مرکز اصلی یا مراکز مجاز
        /// </summary>
        private async Task<bool> HasMinimumDaysInPermittedMarkazAsync(
            BarnamehHaftegiOstad program,
            int ostadId,
            string termCode)
        {
            // دریافت اطلاعات مراکز مجاز با ساختار جدید
            var permittedMarkazInfo = await GetPermittedMarkazInfoAsync(ostadId, termCode);
            var permittedMarkazIds = permittedMarkazInfo.Select(x => x.MarkazId).ToHashSet();

            var groupedByDay = program.BarnamehHaftegiOstad1s
                .GroupBy(d => d.RoozeHafteh)
                .ToList();

            int daysWithPermitted = 0;

            foreach (var day in groupedByDay)
            {
                bool hasPermitted = false;

                foreach (var detail in day)
                {
                    // 🔥 بررسی مرکز اصلی روز (MarkazId)
                    if (detail.MarkazId.HasValue && permittedMarkazIds.Contains(detail.MarkazId.Value))
                    {
                        hasPermitted = true;
                        break;
                    }

                    // بررسی مراکز ساعتی
                    if (detail.MarkazIdA.HasValue && permittedMarkazIds.Contains(detail.MarkazIdA.Value) ||
                        detail.MarkazIdB.HasValue && permittedMarkazIds.Contains(detail.MarkazIdB.Value) ||
                        detail.MarkazIdC.HasValue && permittedMarkazIds.Contains(detail.MarkazIdC.Value) ||
                        detail.MarkazIdD.HasValue && permittedMarkazIds.Contains(detail.MarkazIdD.Value) ||
                        detail.MarkazIdE.HasValue && permittedMarkazIds.Contains(detail.MarkazIdE.Value) ||
                        detail.MarkazIdF.HasValue && permittedMarkazIds.Contains(detail.MarkazIdF.Value) ||
                        detail.MarkazIdG.HasValue && permittedMarkazIds.Contains(detail.MarkazIdG.Value) ||
                        detail.MarkazIdH.HasValue && permittedMarkazIds.Contains(detail.MarkazIdH.Value))
                    {
                        hasPermitted = true;
                        break;
                    }
                }

                if (hasPermitted) daysWithPermitted++;
            }

            return daysWithPermitted >= 5;
        }

        /// <summary>
        /// اعتبارسنجی قیود فعالیت‌ها در برنامه هفتگی
        /// شامل: ساعات اداری (A,B,C)، حداقل/حداکثر ساعت در هفته، حداقل/حداکثر روز در هفته
        /// </summary>
        /// <param name="program">برنامه هفتگی</param>
        /// <param name="ostadId">شناسه استاد</param>
        /// <param name="throwOnWarning">اگر true باشد، هشدارها به‌عنوان خطا در نظر گرفته می‌شوند</param>
        /// <returns>نتیجه اعتبارسنجی همراه با پیام خطا و لیست هشدارها</returns>
        private async Task<(bool IsValid, string ErrorMessage, List<string> Warnings)> ValidateFaaliatConstraintsAsync(
            BarnamehHaftegiOstad program,
            int ostadId,
            bool throwOnWarning = false)
        {
            // ============================================================
            // 1️⃣ دریافت لیست فعالیت‌های فعال از سرویس کش
            // ============================================================
            var faaliats = await GetActiveFaaliatsAsync();
            var faaliatDict = faaliats.ToDictionary(f => f.Id);

            // ============================================================
            // 2️⃣ دریافت لیست ساعت‌های مجاز از سرویس کش
            // ============================================================
            var allSaats = await _saatBargozariCacheService.GetAllActiveAsync();
            var saatCodes = allSaats.Select(s => s.CodeSaat).ToHashSet();

            var warnings = new List<string>();
            var errors = new List<string>();

            // ============================================================
            // 3️⃣ جمع‌آوری آمار فعالیت‌ها
            // ============================================================
            var activityTotalSessions = new Dictionary<int, int>();
            var activityEdariSessions = new Dictionary<int, int>(); // فقط A,B,C
            var activityDays = new Dictionary<int, HashSet<string>>();
            var invalidHours = new List<string>();

            foreach (var detail in program.BarnamehHaftegiOstad1s)
            {
                var fields = new List<(int? Id, string Name)>
            {
                (detail.A, "A"), (detail.B, "B"), (detail.C, "C"),
                (detail.D, "D"), (detail.E, "E"), (detail.F, "F"),
                (detail.G, "G"), (detail.H, "H")
            };

                foreach (var (id, name) in fields)
                {
                    // اگر فعالیتی انتخاب نشده، رد کن
                    if (!id.HasValue || id.Value == 0)
                        continue;

                    // ============================================================
                    // 🔥 بررسی مجاز بودن ساعت
                    // ============================================================
                    if (!saatCodes.Contains(name))
                    {
                        var msg = $"ساعت {name} در سیستم فعال نیست و نمی‌تواند در برنامه استفاده شود";
                        if (throwOnWarning)
                            errors.Add(msg);
                        else
                            warnings.Add(msg);
                        continue;
                    }

                    var activityId = id.Value;

                    // بررسی وجود فعالیت در دیکشنری
                    if (!faaliatDict.ContainsKey(activityId))
                    {
                        var msg = $"فعالیت با شناسه {activityId} در سیستم یافت نشد یا غیرفعال است";
                        if (throwOnWarning)
                            errors.Add(msg);
                        else
                            warnings.Add(msg);
                        continue;
                    }

                    // ============================================================
                    // جمع‌آوری آمار
                    // ============================================================

                    // تعداد کل جلسات
                    if (!activityTotalSessions.ContainsKey(activityId))
                        activityTotalSessions[activityId] = 0;
                    activityTotalSessions[activityId]++;

                    // تعداد جلسات در ساعات اداری (A, B, C)
                    if (name is "A" or "B" or "C")
                    {
                        if (!activityEdariSessions.ContainsKey(activityId))
                            activityEdariSessions[activityId] = 0;
                        activityEdariSessions[activityId]++;
                    }

                    // روزهای حضور
                    if (!activityDays.ContainsKey(activityId))
                        activityDays[activityId] = new HashSet<string>();
                    activityDays[activityId].Add(detail.RoozeHafteh ?? "");
                }
            }

            // ============================================================
            // 4️⃣ اگر خطاهای مربوط به ساعت غیرفعال وجود دارد، برگردان
            // ============================================================
            if (throwOnWarning && errors.Any())
            {
                return (false, string.Join(" | ", errors), warnings);
            }

            // ============================================================
            // 5️⃣ بررسی قیود هر فعالیت
            // ============================================================
            foreach (var activityId in activityTotalSessions.Keys)
            {
                var totalSessions = activityTotalSessions[activityId];
                var totalHours = totalSessions * 2; // هر جلسه = ۲ ساعت
                var edariSessions = activityEdariSessions.GetValueOrDefault(activityId, 0);
                var edariHours = edariSessions * 2;
                var totalDays = activityDays[activityId].Count;
                var faaliat = faaliatDict[activityId];

                // ============================================================
                // 🔸 حداقل ساعت در ساعات اداری (A,B,C)
                // ============================================================
                if (faaliat.MinSaatDarEdari.HasValue && edariHours < faaliat.MinSaatDarEdari.Value)
                {
                    var msg = $"فعالیت '{faaliat.Onvan}' حداقل {faaliat.MinSaatDarEdari} ساعت در ساعات اداری (A,B,C) نیاز دارد (فعلاً {edariHours} ساعت)";
                    if (throwOnWarning)
                        errors.Add(msg);
                    else
                        warnings.Add(msg);
                }

                // ============================================================
                // 🔸 حداکثر ساعت در ساعات اداری (A,B,C)
                // ============================================================
                if (faaliat.MaxSaatDarEdari.HasValue && edariHours > faaliat.MaxSaatDarEdari.Value)
                {
                    var msg = $"فعالیت '{faaliat.Onvan}' حداکثر {faaliat.MaxSaatDarEdari} ساعت در ساعات اداری مجاز است (فعلاً {edariHours} ساعت)";
                    if (throwOnWarning)
                        errors.Add(msg);
                    else
                        warnings.Add(msg);
                }

                // ============================================================
                // 🔸 حداقل ساعت در کل هفته
                // ============================================================
                if (faaliat.MinSaatDarHafteh.HasValue && totalHours < faaliat.MinSaatDarHafteh.Value)
                {
                    var msg = $"فعالیت '{faaliat.Onvan}' حداقل {faaliat.MinSaatDarHafteh} ساعت در هفته نیاز دارد (فعلاً {totalHours} ساعت)";
                    if (throwOnWarning)
                        errors.Add(msg);
                    else
                        warnings.Add(msg);
                }

                // ============================================================
                // 🔸 حداکثر ساعت در کل هفته
                // ============================================================
                if (faaliat.MaxSaatDarHafteh.HasValue && totalHours > faaliat.MaxSaatDarHafteh.Value)
                {
                    var msg = $"فعالیت '{faaliat.Onvan}' حداکثر {faaliat.MaxSaatDarHafteh} ساعت در هفته مجاز است (فعلاً {totalHours} ساعت)";
                    if (throwOnWarning)
                        errors.Add(msg);
                    else
                        warnings.Add(msg);
                }

                // ============================================================
                // 🔸 حداقل روز در هفته
                // ============================================================
                if (faaliat.MinDayDarHafteh.HasValue && totalDays < faaliat.MinDayDarHafteh.Value)
                {
                    var msg = $"فعالیت '{faaliat.Onvan}' باید حداقل در {faaliat.MinDayDarHafteh} روز باشد (فعلاً {totalDays} روز)";
                    if (throwOnWarning)
                        errors.Add(msg);
                    else
                        warnings.Add(msg);
                }

                // ============================================================
                // 🔸 حداکثر روز در هفته
                // ============================================================
                if (faaliat.MaxDayDarHafteh.HasValue && totalDays > faaliat.MaxDayDarHafteh.Value)
                {
                    var msg = $"فعالیت '{faaliat.Onvan}' حداکثر در {faaliat.MaxDayDarHafteh} روز مجاز است (فعلاً {totalDays} روز)";
                    if (throwOnWarning)
                        errors.Add(msg);
                    else
                        warnings.Add(msg);
                }
            }

            // ============================================================
            // 6️⃣ بررسی اینکه آیا برنامه حداقل یک فعالیت دارد
            // ============================================================
            if (activityTotalSessions.Count == 0)
            {
                var msg = "برنامه هفتگی هیچ فعالیتی ندارد";
                if (throwOnWarning)
                    return (false, msg, warnings);
                else
                    warnings.Add(msg);
            }

            // ============================================================
            // 7️⃣ نتیجه نهایی
            // ============================================================
            if (throwOnWarning && errors.Any())
            {
                return (false, string.Join(" | ", errors), warnings);
            }

            return (true, "", warnings);
        }

        /// <summary>
        /// بررسی کامل بودن برنامه (حداقل ساعت موظفی + ۵ روز در مراکز مجاز)
        /// </summary>
        private async Task<(bool IsValid, string Message)> ValidateProgramCompletenessAsync(
            BarnamehHaftegiOstad program,
            int ostadId,
            string termCode)
        {
            // 1️⃣ بررسی حداقل ساعت موظفی
            var requiredHours = await GetRequiredHoursAsync(ostadId, termCode);
            var totalSessions = CalculateTotalSessions(program);
            var requiredSessions = requiredHours / 2;

            if (totalSessions < requiredSessions)
            {
                return (false, $"حداقل {requiredSessions} جلسه ({requiredHours} ساعت) باید پر شود. تعداد جلسات فعلی: {totalSessions}");
            }

            // 2️⃣ بررسی شرط ۵ روز در مرکز اصلی یا مراکز مجاز
            var hasMinimumDays = await HasMinimumDaysInPermittedMarkazAsync(program, ostadId, termCode);
            if (!hasMinimumDays)
            {
                return (false, "حداقل ۵ روز از برنامه باید در مرکز اصلی یا مراکز مجاز در همجوار پر شده باشد");
            }

            return (true, "");
        }

        /// <summary>
        /// ساخت برنامه موقت برای اعتبارسنجی بدون ذخیره در دیتابیس
        /// </summary>
        private BarnamehHaftegiOstad BuildTemporaryProgram(
            int ostadId,
            string termCode,
            List<BarnamehHaftegiDetailCreateDto> details)
        {
            var program = new BarnamehHaftegiOstad
            {
                OstadId = ostadId,
                CodeTerm = termCode,
                BarnamehHaftegiOstad1s = details.Select(d => new BarnamehHaftegiOstad1
                {
                    RoozeHafteh = d.RoozeHafteh,
                    A = d.A,
                    MarkazIdA = d.MarkazIdA,
                    B = d.B,
                    MarkazIdB = d.MarkazIdB,
                    C = d.C,
                    MarkazIdC = d.MarkazIdC,
                    D = d.D,
                    MarkazIdD = d.MarkazIdD,
                    E = d.E,
                    MarkazIdE = d.MarkazIdE,
                    F = d.F,
                    MarkazIdF = d.MarkazIdF,
                    G = d.G,
                    MarkazIdG = d.MarkazIdG,
                    H = d.H,
                    MarkazIdH = d.MarkazIdH,
                    Jozeiat = d.Jozeiat
                }).ToList()
            };

            return program;
        }

        /// <summary>
        /// دریافت وضعیت ترکیبی برنامه برای نمایش در لیست
        /// </summary>
        private string GetApproveStatus(BarnamehHaftegiOstad program)
        {
            if (program.NazarMoaven == 1) return "tayeed_moaven";
            if (program.NazarModirGrooh == 1) return "tayeed_modir";
            if (program.NazarElmi == 1) return "tayeed_ostad";
            return "pishnevis";
        }

        private string GetApproveStatusDisplay(string status)
        {
            return status switch
            {
                "pishnevis" => "پیش‌نویس",
                "tayeed_ostad" => "تایید استاد",
                "tayeed_modir" => "تایید مدیر گروه",
                "tayeed_moaven" => "تایید معاون آموزشی استان",
                "no_program" => "فاقد برنامه",
                _ => "نامشخص"
            };
        }

        /// <summary>
        /// اعتبارسنجی کامل برنامه (مراکز مجاز، فعالیت‌های مجاز و IsMadove)
        /// </summary>
        private async Task<(bool IsValid, string Message)> ValidateProgramDetailsAsync(
            List<BarnamehHaftegiDetailCreateDto> details,
            int ostadId,
            string termCode,
            bool isUpdate = false)
        {
            // ============================================================
            // 1️⃣ دریافت اطلاعات مراکز مجاز
            // ============================================================
            var permittedMarkazInfo = await GetPermittedMarkazInfoAsync(ostadId, termCode);
            var permittedDict = permittedMarkazInfo.ToDictionary(x => x.MarkazId);

            // 2️⃣ دریافت اطلاعات استاد برای نوع همکاری (IsMadove)
            var ostad = await _context.Ostads.FirstOrDefaultAsync(o => o.Id == ostadId);
            if (ostad == null)
                return (false, "استاد یافت نشد");
            bool isHeyatElmi = ostad.NoeHamkari == NoeHamkariEnum.HeyatElmiPayamNoor;

            // 3️⃣ دریافت لیست فعالیت‌های فعال
            var allFaaliats = await GetActiveFaaliatsAsync();
            var faaliatDict = allFaaliats.ToDictionary(f => f.Id);

            // 4️⃣ دریافت لیست ساعت‌های مجاز
            var allSaats = await _saatBargozariCacheService.GetAllActiveAsync();
            var saatDict = allSaats.ToDictionary(s => s.CodeSaat);

            // 5️⃣ دریافت لیست مراکز با قابلیت مجازی
            var allMarkaz = await _markazCache.GetAllAsync();
            var virtualMarkazIds = allMarkaz
                .Where(m => m.NoeMarkaz == 2 || m.NoeMarkaz == 3)
                .Select(m => m.Id)
                .ToHashSet();

            // 6️⃣ شمارش تعداد روزهای استفاده از هر مرکز غیراصلی
            var nonMainMarkazUsage = new Dictionary<int, int>();

            // ============================================================
            // 7️⃣ اعتبارسنجی مرکز اصلی هر روز
            // ============================================================
            foreach (var detail in details)
            {
                // بررسی مجاز بودن MarkazId روز
                if (!permittedDict.ContainsKey(detail.MarkazId))
                    return (false, $"مرکز انتخاب‌شده برای روز {GetDayDisplay(detail.RoozeHafteh)} مجاز نیست");

                var markazInfo = permittedDict[detail.MarkazId];

                // اگر مرکز غیراصلی است، تعداد روزهای استفاده را بررسی کن
                if (!markazInfo.IsMainMarkaz)
                {
                    if (!nonMainMarkazUsage.ContainsKey(detail.MarkazId))
                        nonMainMarkazUsage[detail.MarkazId] = 0;

                    nonMainMarkazUsage[detail.MarkazId]++;

                    if (nonMainMarkazUsage[detail.MarkazId] > markazInfo.MaxDays)
                        return (false, $"تعداد روزهای استفاده از مرکز {GetMarkazName(detail.MarkazId, allMarkaz)} بیش از حد مجاز ({markazInfo.MaxDays} روز) است");
                }
            }

            // ============================================================
            // 8️⃣ اعتبارسنجی جزئیات ساعتی
            // ============================================================
            foreach (var detail in details)
            {
                var dayMarkazId = detail.MarkazId;
                var dayMarkazInfo = permittedDict[dayMarkazId];
                var isMainMarkaz = dayMarkazInfo.IsMainMarkaz;

                // برای مراکز غیراصلی، بررسی حداقل ۳ جلسه در همان مرکز
                if (!isMainMarkaz)
                {
                    var mainMarkazSessionCount = 0;
                    var hourFields = new List<int?>
                    {
                        detail.MarkazIdA, detail.MarkazIdB, detail.MarkazIdC,
                        detail.MarkazIdD, detail.MarkazIdE, detail.MarkazIdF,
                        detail.MarkazIdG, detail.MarkazIdH
                    };

                    mainMarkazSessionCount = hourFields.Count(id => id.HasValue && id.Value == dayMarkazId);

                    if (mainMarkazSessionCount < 3)
                        return (false, $"در روز {GetDayDisplay(detail.RoozeHafteh)}، برای مرکز غیراصلی باید حداقل ۳ جلسه (۶ ساعت) در همان مرکز باشد");
                }

                // اعتبارسنجی هر ساعت
                    var hourFieldsWithActivity = new List<(int? ActivityId, int? MarkazId, string FieldName, string CodeSaat)>
                    {
                        (detail.A, detail.MarkazIdA, "A", "A"),
                        (detail.B, detail.MarkazIdB, "B", "B"),
                        (detail.C, detail.MarkazIdC, "C", "C"),
                        (detail.D, detail.MarkazIdD, "D", "D"),
                        (detail.E, detail.MarkazIdE, "E", "E"),
                        (detail.F, detail.MarkazIdF, "F", "F"),
                        (detail.G, detail.MarkazIdG, "G", "G"),
                        (detail.H, detail.MarkazIdH, "H", "H"),
                    };

                foreach (var (activityId, markazIdX, fieldName, codeSaat) in hourFieldsWithActivity)
                {
                    // اگر فعالیتی انتخاب نشده، ادامه بده
                    if (!activityId.HasValue || activityId.Value == 0)
                        continue;

                    // اگر مرکز ساعت انتخاب نشده، خطا
                    if (!markazIdX.HasValue)
                        return (false, $"در ساعت {fieldName} فعالیت انتخاب شده اما مرکز مشخص نشده است");

                    // بررسی مجاز بودن مرکز ساعت
                    var faaliat = faaliatDict.GetValueOrDefault(activityId.Value);
                    if (faaliat == null)
                        return (false, $"فعالیت با شناسه {activityId} یافت نشد");

                    // تشخیص نوع فعالیت (حضوری/مجازی)
                    bool isVirtualActivity = faaliat.NoeAnjam == 2 || faaliat.NoeAnjam == 3;
                    bool isHozooriActivity = faaliat.NoeAnjam == 1 || faaliat.NoeAnjam == 3;

                    // قانون ۱: فعالیت حضوری → MarkazIdX باید برابر با MarkazId روز باشد
                    if (isHozooriActivity && markazIdX.Value != dayMarkazId)
                        return (false, $"در ساعت {fieldName}، فعالیت حضوری باید در مرکز اصلی روز ({GetMarkazName(dayMarkazId, allMarkaz)}) باشد");

                    // قانون ۲: فعالیت مجازی → مرکز باید قابلیت مجازی داشته باشد
                    if (isVirtualActivity && !virtualMarkazIds.Contains(markazIdX.Value))
                        return (false, $"مرکز {GetMarkazName(markazIdX.Value, allMarkaz)} قابلیت مجازی ندارد");

                    // قانون ۳: اگر مرکز غیراصلی است، فعالیت‌های حضوری باید در لیست مجاز باشند
                    if (!isMainMarkaz && isHozooriActivity)
                    {
                        var allowedFaaliatIds = dayMarkazInfo.AllowedFaaliatIds;
                        if (!allowedFaaliatIds.Contains(activityId.Value))
                            return (false, $"فعالیت '{faaliat.Onvan}' برای مرکز {GetMarkazName(dayMarkazId, allMarkaz)} مجاز نیست");
                    }

                    // قانون ۴: اگر مرکز غیراصلی است و فعالیت مجازی است، نیازی به مجوز خاصی ندارد (قبلاً بررسی شده)

                    // قانون ۵: بررسی ساعت مجاز
                    var saat = saatDict.GetValueOrDefault(codeSaat);
                    if (saat == null)
                        return (false, $"ساعت {fieldName} در سیستم تعریف نشده است");

                    if (isHozooriActivity && saat.Hozoori != true)
                        return (false, $"ساعت {fieldName} برای فعالیت حضوری مجاز نیست");

                    if (isVirtualActivity && saat.Majazi != true)
                        return (false, $"ساعت {fieldName} برای فعالیت مجازی مجاز نیست");
                }

                // قانون ۶: بررسی IsMadove برای مدرس مدعو
                if (!isHeyatElmi)
                {
                    var allActivityIds = new List<int?>
                    {
                        detail.A, detail.B, detail.C, detail.D,
                        detail.E, detail.F, detail.G, detail.H
                    }
                    .Where(id => id.HasValue && id.Value > 0)
                    .Select(id => id.Value)
                    .Distinct()
                    .ToList();

                    foreach (var actId in allActivityIds)
                    {
                        if (faaliatDict.ContainsKey(actId) && faaliatDict[actId].IsMadove != true)
                            return (false, $"مدرس مدعو نمی‌تواند از فعالیت '{faaliatDict[actId].Onvan}' استفاده کند");
                    }
                }
            }

            return (true, "");
        }

        // BarnamehHaftegiController.cs
        [HttpGet("active-saats")]
        [AllowAnonymous]
        public async Task<IActionResult> GetActiveSaats(
            [FromQuery] int? noeAnjam = null) // 1=حضوری, 2=مجازی, 3=ترکیبی
        {
            try
            {
                var saats = await _saatBargozariCacheService.GetAllActiveAsync();

                if (noeAnjam.HasValue)
                {
                    saats = noeAnjam.Value switch
                    {
                        1 => saats.Where(s => s.Hozoori == true).ToList(),
                        2 => saats.Where(s => s.Majazi == true).ToList(),
                        3 => saats.Where(s => s.Hozoori == true || s.Majazi == true).ToList(),
                        _ => saats
                    };
                }

                return Ok(new
                {
                    success = true,
                    message = "لیست ساعت‌های برگزاری کلاس دریافت شد",
                    data = saats.Select(s => new
                    {
                        s.Id,
                        s.CodeSaat,
                        s.OnvanSaat,
                        s.SaatShoroo,
                        s.SaatPayan,
                        s.Hozoori,
                        s.Majazi
                    })
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت ساعت‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 1️⃣ ایجاد برنامه هفتگی
        // ============================================================
        [HttpPost("create")]
        public async Task<IActionResult> Create([FromBody] BarnamehHaftegiCreateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ بررسی دسترسی
                // ============================================================
                var isOstad = currentRole?.Name == "استاد";
                if (!isOstad)
                    return Forbid();

                if (currentUser.OstadId != dto.OstadId)
                    return BadRequest(new { success = false, message = "شما فقط می‌توانید برنامه خود را ایجاد کنید" });

                // ============================================================
                // 2️⃣ بررسی یکتایی
                // ============================================================
                var exists = await _context.BarnamehHaftegiOstads
                    .AnyAsync(b => b.OstadId == dto.OstadId && b.CodeTerm == dto.CodeTerm);

                if (exists)
                    return BadRequest(new { success = false, message = "این استاد قبلاً برای این ترم برنامه ثبت کرده است" });

                // ============================================================
                // 3️⃣ اعتبارسنجی ترم
                // ============================================================
                var term = await _context.Terms
                    .FirstOrDefaultAsync(t => t.CodeTerm == dto.CodeTerm);

                if (term == null)
                    return BadRequest(new { success = false, message = "ترم وارد شده معتبر نیست" });

                // ============================================================
                // 4️⃣ اعتبارسنجی ساختاری (مراکز مجاز، IsMadove و ...)
                // ============================================================
                if (dto.Details == null || !dto.Details.Any())
                    return BadRequest(new { success = false, message = "حداقل یک روز باید ثبت شود" });

                var validation = await ValidateProgramDetailsAsync(dto.Details, dto.OstadId, dto.CodeTerm);
                if (!validation.IsValid)
                    return BadRequest(new { success = false, message = validation.Message });

                // ============================================================
                // 5️⃣ اعتبارسنجی قیود فعالیت‌ها (فقط هشدار)
                // ============================================================
                var tempProgram = BuildTemporaryProgram(dto.OstadId, dto.CodeTerm, dto.Details);
                var (_, _, warnings) = await ValidateFaaliatConstraintsAsync(tempProgram, dto.OstadId, false);

                // ============================================================
                // 6️⃣ ذخیره‌سازی
                // ============================================================
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    var program = new BarnamehHaftegiOstad
                    {
                        OstadId = dto.OstadId,
                        CodeTerm = dto.CodeTerm,
                        NazarElmi = 0,
                        NazarModirGrooh = 0,
                        NazarMoaven = 0,
                        IsLocked = false,
                        TarikhElmi = DateTime.UtcNow
                    };

                    await _context.BarnamehHaftegiOstads.AddAsync(program);
                    await _context.SaveChangesAsync();

                    foreach (var detail in dto.Details)
                    {
                        var detailEntity = new BarnamehHaftegiOstad1
                        {
                            BarnamehHaftegiOstadId = program.Id,
                            RoozeHafteh = detail.RoozeHafteh,
                            A = detail.A,
                            MarkazIdA = detail.MarkazIdA,
                            B = detail.B,
                            MarkazIdB = detail.MarkazIdB,
                            C = detail.C,
                            MarkazIdC = detail.MarkazIdC,
                            D = detail.D,
                            MarkazIdD = detail.MarkazIdD,
                            E = detail.E,
                            MarkazIdE = detail.MarkazIdE,
                            F = detail.F,
                            MarkazIdF = detail.MarkazIdF,
                            G = detail.G,
                            MarkazIdG = detail.MarkazIdG,
                            H = detail.H,
                            MarkazIdH = detail.MarkazIdH,
                            Jozeiat = detail.Jozeiat
                        };
                        await _context.BarnamehHaftegiOstad1s.AddAsync(detailEntity);
                    }

                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    var response = new
                    {
                        success = true,
                        message = warnings.Any()
                            ? "برنامه با هشدار ذخیره شد. لطفاً قبل از تأیید، هشدارها را برطرف کنید."
                            : "برنامه هفتگی با موفقیت ایجاد شد",
                        data = new { id = program.Id },
                        warnings = warnings.Any() ? warnings : null,
                        hasWarnings = warnings.Any()
                    };

                    return Ok(response);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ایجاد برنامه هفتگی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 2️⃣ ویرایش برنامه هفتگی
        // ============================================================
        [HttpPut("update/{id}")]
        public async Task<IActionResult> Update(int id, [FromBody] BarnamehHaftegiUpdateDto dto)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ دریافت برنامه
                // ============================================================
                var program = await _context.BarnamehHaftegiOstads
                    .Include(b => b.BarnamehHaftegiOstad1s)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه یافت نشد" });

                // ============================================================
                // 2️⃣ بررسی دسترسی
                // ============================================================
                var isOstad = currentRole?.Name == "استاد";

                if (isOstad)
                {
                    if (currentUser.OstadId != program.OstadId)
                        return Forbid();

                    if (program.NazarElmi != 0 || program.IsLocked)
                        return BadRequest(new { success = false, message = "برنامه تأیید شده و قابل ویرایش نیست" });
                }
                else
                {
                    if (!await _accessService.CanAccessTargetOstadAsync(program.OstadId, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                // ============================================================
                // 3️⃣ اعتبارسنجی ساختاری
                // ============================================================
                if (dto.Details == null || !dto.Details.Any())
                    return BadRequest(new { success = false, message = "حداقل یک روز باید ثبت شود" });

                var validation = await ValidateProgramDetailsAsync(dto.Details, program.OstadId, program.CodeTerm, true);
                if (!validation.IsValid)
                    return BadRequest(new { success = false, message = validation.Message });

                // ============================================================
                // 4️⃣ اعتبارسنجی قیود فعالیت‌ها (فقط هشدار)
                // ============================================================
                var tempProgram = BuildTemporaryProgram(program.OstadId, program.CodeTerm, dto.Details);
                var (_, _, warnings) = await ValidateFaaliatConstraintsAsync(tempProgram, program.OstadId, false);

                // ============================================================
                // 5️⃣ به‌روزرسانی
                // ============================================================
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    _context.BarnamehHaftegiOstad1s.RemoveRange(program.BarnamehHaftegiOstad1s);

                    foreach (var detail in dto.Details)
                    {
                        var detailEntity = new BarnamehHaftegiOstad1
                        {
                            BarnamehHaftegiOstadId = program.Id,
                            RoozeHafteh = detail.RoozeHafteh,
                            A = detail.A,
                            MarkazIdA = detail.MarkazIdA,
                            B = detail.B,
                            MarkazIdB = detail.MarkazIdB,
                            C = detail.C,
                            MarkazIdC = detail.MarkazIdC,
                            D = detail.D,
                            MarkazIdD = detail.MarkazIdD,
                            E = detail.E,
                            MarkazIdE = detail.MarkazIdE,
                            F = detail.F,
                            MarkazIdF = detail.MarkazIdF,
                            G = detail.G,
                            MarkazIdG = detail.MarkazIdG,
                            H = detail.H,
                            MarkazIdH = detail.MarkazIdH,
                            Jozeiat = detail.Jozeiat
                        };
                        await _context.BarnamehHaftegiOstad1s.AddAsync(detailEntity);
                    }

                    program.TarikhElmi = DateTime.UtcNow;
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    _cache.Remove($"PermittedMarkaz_{program.OstadId}_{program.CodeTerm}");

                    var response = new
                    {
                        success = true,
                        message = warnings.Any()
                            ? "برنامه با هشدار ذخیره شد. لطفاً قبل از تأیید، هشدارها را برطرف کنید."
                            : "برنامه هفتگی با موفقیت ویرایش شد",
                        warnings = warnings.Any() ? warnings : null,
                        hasWarnings = warnings.Any()
                    };

                    return Ok(response);
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ویرایش برنامه هفتگی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 3️⃣ حذف برنامه هفتگی
        // ============================================================
        [HttpDelete("delete/{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ دریافت برنامه
                // ============================================================
                var program = await _context.BarnamehHaftegiOstads
                    .Include(b => b.BarnamehHaftegiOstad1s)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه یافت نشد" });

                // ============================================================
                // 2️⃣ بررسی دسترسی
                // ============================================================
                var isOstad = currentRole?.Name == "استاد";

                if (isOstad)
                {
                    // استاد فقط می‌تواند برنامه خود را حذف کند
                    if (currentUser.OstadId != program.OstadId)
                        return Forbid();

                    // فقط در حالت پیش‌نویس قابل حذف است
                    if (program.NazarElmi != 0 || program.IsLocked)
                        return BadRequest(new { success = false, message = "برنامه تأیید شده و قابل حذف نیست" });
                }
                else if (codeRole != 1)
                {
                    // فقط ادمین سامانه می‌تواند برنامه دیگران را حذف کند
                    return Forbid();
                }

                // ============================================================
                // 3️⃣ حذف
                // ============================================================
                using var transaction = await _context.Database.BeginTransactionAsync();

                try
                {
                    if (program.BarnamehHaftegiOstad1s != null && program.BarnamehHaftegiOstad1s.Any())
                    {
                        _context.BarnamehHaftegiOstad1s.RemoveRange(program.BarnamehHaftegiOstad1s);
                    }

                    _context.BarnamehHaftegiOstads.Remove(program);
                    await _context.SaveChangesAsync();
                    await transaction.CommitAsync();

                    // پاک کردن کش
                    _cache.Remove($"PermittedMarkaz_{program.OstadId}_{program.CodeTerm}");

                    return Ok(new
                    {
                        success = true,
                        message = "برنامه هفتگی با موفقیت حذف شد"
                    });
                }
                catch
                {
                    await transaction.RollbackAsync();
                    throw;
                }
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در حذف برنامه هفتگی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 4️⃣ دریافت یک برنامه با ID
        // ============================================================
        [HttpGet("{id}")]
        public async Task<IActionResult> GetById(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ دریافت برنامه
                // ============================================================
                var program = await _context.BarnamehHaftegiOstads
                    .Include(b => b.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .Include(b => b.Term)
                    .Include(b => b.BarnamehHaftegiOstad1s)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه ای یافت نشد" });

                // ============================================================
                // 2️⃣ بررسی دسترسی
                // ============================================================
                var isOstad = currentRole?.Name == "استاد";

                if (isOstad)
                {
                    if (currentUser.OstadId != program.OstadId)
                        return Forbid();
                }
                else
                {
                    if (!await _accessService.CanAccessTargetOstadAsync(program.OstadId, codeRole.Value, currentMarkaz?.Id))
                        return Forbid();
                }

                // ============================================================
                // 3️⃣ دریافت اطلاعات مراکز مجاز (با ساختار جدید)
                // ============================================================
                var permittedMarkazInfo = await GetPermittedMarkazInfoAsync(program.OstadId, program.CodeTerm);
                var permittedMarkazIds = permittedMarkazInfo.Select(x => x.MarkazId).ToList();

                var allMarkaz = await _markazCache.GetAllAsync();

                // ============================================================
                // 4️⃣ محاسبات تکمیلی
                // ============================================================
                var requiredHours = await GetRequiredHoursAsync(program.OstadId, program.CodeTerm);
                var totalSessions = CalculateTotalSessions(program);
                var requiredSessions = requiredHours / 2;
                var isComplete = totalSessions >= requiredSessions;

                // ============================================================
                // 5️⃣ ساخت خروجی
                // ============================================================
                var dto = new BarnamehHaftegiDetailDto
                {
                    Id = program.Id,
                    OstadId = program.OstadId,
                    OstadName = $"{program.Ostad?.Naam} {program.Ostad?.NaamKhanevadegi}".Trim(),
                    OstadCode = program.Ostad?.CodeOstadi ?? "",
                    OstadMarkaz = program.Ostad?.Markaz?.NaamMarkaz ?? "",
                    TermTitle = program.Term?.OnvanTerm ?? "",
                    CodeTerm = program.CodeTerm,

                    NazarElmi = program.NazarElmi,
                    NazarElmiDisplay = GetNazarDisplay(program.NazarElmi),
                    NazarModirGrooh = program.NazarModirGrooh,
                    NazarModirGroohDisplay = GetNazarDisplay(program.NazarModirGrooh),
                    NazarMoaven = program.NazarMoaven,
                    NazarMoavenDisplay = GetNazarDisplay(program.NazarMoaven),

                    IsLocked = program.IsLocked,
                    ApproveStatus = GetApproveStatus(program),
                    ApproveStatusDisplay = GetApproveStatusDisplay(GetApproveStatus(program)),

                    TarikhElmi = program.TarikhElmi,
                    TarikhModirGrooh = program.TarikhModirGrooh,
                    TarikhMoaven = program.TarikhMoaven,

                    TotalSessions = totalSessions,
                    RequiredSessions = requiredSessions,
                    RequiredHours = requiredHours,
                    IsComplete = isComplete,

                    Details = program.BarnamehHaftegiOstad1s
                        .OrderBy(d => d.RoozeHafteh)
                        .Select(d => new BarnamehHaftegiDetailItemDto
                        {
                            Id = d.Id,
                            RoozeHafteh = d.RoozeHafteh ?? "",
                            RoozeHaftehDisplay = GetDayDisplay(d.RoozeHafteh),
                            MarkazId = d.MarkazId,  // 🔥 اضافه شد
                            MarkazName = GetMarkazName(d.MarkazId, allMarkaz),  // 🔥 اضافه شد
                            A = d.A,
                            MarkazIdA = d.MarkazIdA,
                            MarkazNameA = GetMarkazName(d.MarkazIdA, allMarkaz),
                            B = d.B,
                            MarkazIdB = d.MarkazIdB,
                            MarkazNameB = GetMarkazName(d.MarkazIdB, allMarkaz),
                            C = d.C,
                            MarkazIdC = d.MarkazIdC,
                            MarkazNameC = GetMarkazName(d.MarkazIdC, allMarkaz),
                            D = d.D,
                            MarkazIdD = d.MarkazIdD,
                            MarkazNameD = GetMarkazName(d.MarkazIdD, allMarkaz),
                            E = d.E,
                            MarkazIdE = d.MarkazIdE,
                            MarkazNameE = GetMarkazName(d.MarkazIdE, allMarkaz),
                            F = d.F,
                            MarkazIdF = d.MarkazIdF,
                            MarkazNameF = GetMarkazName(d.MarkazIdF, allMarkaz),
                            G = d.G,
                            MarkazIdG = d.MarkazIdG,
                            MarkazNameG = GetMarkazName(d.MarkazIdG, allMarkaz),
                            H = d.H,
                            MarkazIdH = d.MarkazIdH,
                            MarkazNameH = GetMarkazName(d.MarkazIdH, allMarkaz),
                            Jozeiat = d.Jozeiat,
                            IsPermittedDay = IsDayPermitted(d, permittedMarkazInfo)  // 🔥 اصلاح شد
                        })
                        .ToList()
                };

                return Ok(new
                {
                    success = true,
                    message = "اطلاعات برنامه دریافت شد",
                    data = dto
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در دریافت اطلاعات برنامه",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 5️⃣ لیست برنامه‌ها با فیلتر و صفحه‌بندی
        // ============================================================
        [HttpGet("list")]
        public async Task<IActionResult> GetList(
            [FromQuery] string termCode,
            [FromQuery] string? search = null,
            [FromQuery] int? ostanId = null,
            [FromQuery] int? markazId = null,
            [FromQuery] string? reshteh = null,
            [FromQuery] string? approveStatus = null,
            [FromQuery] int? grooheAmoozeshiId = null,
            [FromQuery] int? noeHamkari = null,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // 1️⃣ تعیین ترم
                if (string.IsNullOrEmpty(termCode))
                {
                    var activeTerm = await _context.Terms.FirstOrDefaultAsync(t => t.Vazeeyat == true);
                    if (activeTerm == null)
                        return BadRequest(new { success = false, message = "ترم جاری در سیستم تعریف نشده است" });
                    termCode = activeTerm.CodeTerm!;
                }

                // 2️⃣ ساخت کوئری اساتید
                var ostadQuery = _context.Ostads
                    .Include(o => o.Markaz)
                    .Include(o => o.OstadMadraks.Where(m => m.PishFarz == true))
                    .AsQueryable();

                // 3️⃣ محدودیت دسترسی
                var isOstad = currentRole?.Name == "استاد";

                if (isOstad)
                {
                    ostadQuery = ostadQuery.Where(o => o.Id == currentUser.OstadId);
                }
                else if (codeRole == 1 || codeRole == 2)
                {
                    // بدون محدودیت
                }
                else if (codeRole == 3 || codeRole == 4)
                {
                    if (currentMarkaz == null || string.IsNullOrEmpty(currentMarkaz.CodeOstan))
                        return Forbid("اطلاعات استان کاربر کامل نیست");

                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == currentMarkaz.CodeOstan && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    ostadQuery = ostadQuery.Where(o => o.MarkazId.HasValue && markazIdsInOstan.Contains(o.MarkazId.Value));
                }
                else
                {
                    var accessibleMarkazIds = await _accessService.GetAccessibleMarkazIdsAsync(codeRole.Value, currentMarkaz?.Id);
                    ostadQuery = ostadQuery.Where(o => o.MarkazId.HasValue && accessibleMarkazIds.Contains(o.MarkazId.Value));
                }

                // 4️⃣ فیلتر جستجو
                if (!string.IsNullOrEmpty(search))
                {
                    search = search.Trim();
                    ostadQuery = ostadQuery.Where(o =>
                        (o.Naam != null && o.Naam.Contains(search)) ||
                        (o.NaamKhanevadegi != null && o.NaamKhanevadegi.Contains(search)) ||
                        (o.CodeOstadi != null && o.CodeOstadi.Contains(search)));
                }

                // 5️⃣ فیلتر استان و مرکز
                if (ostanId.HasValue && !markazId.HasValue)
                {
                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanId.Value.ToString() && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();
                    ostadQuery = ostadQuery.Where(o => o.MarkazId.HasValue && markazIdsInOstan.Contains(o.MarkazId.Value));
                }
                else if (ostanId.HasValue && markazId.HasValue)
                {
                    ostadQuery = ostadQuery.Where(o => o.MarkazId == markazId.Value);
                }

                // 6️⃣ فیلتر گروه آموزشی و رشته
                if (grooheAmoozeshiId.HasValue)
                {
                    ostadQuery = ostadQuery.Where(o =>
                        o.OstadMadraks.Any(m => m.PishFarz == true && m.GrooheAmoozeshiId == grooheAmoozeshiId.Value));
                }

                if (!string.IsNullOrEmpty(reshteh))
                {
                    reshteh = reshteh.Trim();
                    ostadQuery = ostadQuery.Where(o =>
                        o.OstadMadraks.Any(m => m.PishFarz == true && m.Reshteh != null && m.Reshteh.Contains(reshteh)));
                }

                // 4️⃣ فیلتر نوع همکاری
                if (noeHamkari.HasValue)
                {
                    ostadQuery = ostadQuery.Where(o => o.NoeHamkari == (NoeHamkariEnum)noeHamkari.Value);
                }

                // 7️⃣ فیلتر وضعیت برنامه (با استفاده از Subquery)
                if (!string.IsNullOrEmpty(approveStatus))
                {
                    ostadQuery = approveStatus switch
                    {
                        "pishnevis" => ostadQuery.Where(o =>
                            _context.BarnamehHaftegiOstads.Any(b =>
                                b.OstadId == o.Id &&
                                b.CodeTerm == termCode &&
                                b.NazarElmi == 0 &&
                                b.NazarModirGrooh == 0 &&
                                b.NazarMoaven == 0)),
                        "tayeed_ostad" => ostadQuery.Where(o =>
                            _context.BarnamehHaftegiOstads.Any(b =>
                                b.OstadId == o.Id &&
                                b.CodeTerm == termCode &&
                                b.NazarElmi == 1 &&
                                b.NazarModirGrooh == 0 &&
                                b.NazarMoaven == 0)),
                        "tayeed_modir" => ostadQuery.Where(o =>
                            _context.BarnamehHaftegiOstads.Any(b =>
                                b.OstadId == o.Id &&
                                b.CodeTerm == termCode &&
                                b.NazarModirGrooh == 1 &&
                                b.NazarMoaven == 0)),
                        "tayeed_moaven" => ostadQuery.Where(o =>
                            _context.BarnamehHaftegiOstads.Any(b =>
                                b.OstadId == o.Id &&
                                b.CodeTerm == termCode &&
                                b.NazarMoaven == 1)),
                        "no_program" => ostadQuery.Where(o =>
                            !_context.BarnamehHaftegiOstads.Any(b =>
                                b.OstadId == o.Id &&
                                b.CodeTerm == termCode)),
                        _ => ostadQuery
                    };
                }

                // 8️⃣ صفحه‌بندی
                var totalCount = await ostadQuery.CountAsync();

                var ostads = await ostadQuery
                    .OrderBy(o => o.NaamKhanevadegi)
                    .ThenBy(o => o.Naam)
                    .Skip((page - 1) * pageSize)
                    .Take(pageSize)
                    .Select(o => new
                    {
                        Ostad = o,
                        PishFarzMadrak = o.OstadMadraks.FirstOrDefault(m => m.PishFarz == true),
                        Program = _context.BarnamehHaftegiOstads
                            .FirstOrDefault(b => b.OstadId == o.Id && b.CodeTerm == termCode)
                    })
                    .ToListAsync();

                // 9️⃣ ساخت خروجی
                var result = ostads.Select(x =>
                {
                    var program = x.Program;

                    return new BarnamehHaftegiListDto
                    {
                        OstadId = x.Ostad.Id,
                        OstadName = $"{x.Ostad.Naam} {x.Ostad.NaamKhanevadegi}".Trim(),
                        OstadCode = x.Ostad.CodeOstadi ?? "",
                        OstadMarkaz = x.Ostad.Markaz != null ? x.Ostad.Markaz.NaamMarkaz ?? "" : "",
                        NoeHamkari = (int)(x.Ostad.NoeHamkari ?? 0),
                        MartabeElmi = x.Ostad.MartabeElmi,
                        Maghta = x.PishFarzMadrak?.Maghta,
                        Reshteh = x.PishFarzMadrak?.Reshteh,
                        GrooheAmoozeshiId = x.PishFarzMadrak?.GrooheAmoozeshiId,

                        HasProgram = program != null,
                        ApproveStatus = program != null ? GetApproveStatus(program) : "no_program",
                        ApproveStatusDisplay = program != null ? GetApproveStatusDisplay(GetApproveStatus(program)) : "فاقد برنامه",

                        ProgramId = program?.Id,
                        NazarElmi = program?.NazarElmi,
                        NazarModirGrooh = program?.NazarModirGrooh,
                        NazarMoaven = program?.NazarMoaven,
                        IsLocked = program?.IsLocked ?? false,
                        CreatedAt = program?.TarikhElmi
                    };
                }).ToList();

                return Ok(new
                {
                    success = true,
                    message = "لیست برنامه‌های هفتگی دریافت شد",
                    data = result,
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
                    message = "خطا در دریافت لیست برنامه‌های هفتگی",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 6️⃣ تأیید توسط استاد
        // ============================================================
        [HttpPatch("confirm/ostad/{id}")]
        public async Task<IActionResult> ConfirmByOstad(int id)
        {
            try
            {
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ============================================================
                // 1️⃣ دریافت برنامه
                // ============================================================
                var program = await _context.BarnamehHaftegiOstads
                    .Include(b => b.BarnamehHaftegiOstad1s)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه یافت نشد" });

                // ============================================================
                // 2️⃣ بررسی دسترسی
                // ============================================================
                var isOstad = currentRole?.Name == "استاد";
                if (!isOstad || currentUser.OstadId != program.OstadId)
                    return Forbid();

                // ============================================================
                // 3️⃣ بررسی وضعیت
                // ============================================================
                if (program.NazarElmi != 0)
                    return BadRequest(new { success = false, message = "این برنامه قبلاً تأیید شده است" });

                if (program.IsLocked)
                    return BadRequest(new { success = false, message = "این برنامه قفل شده است" });

                // ============================================================
                // 4️⃣ اعتبارسنجی کامل قیود فعالیت‌ها (با خطا)
                // ============================================================
                var (faaliatValid, faaliatError, faaliatWarnings) = await ValidateFaaliatConstraintsAsync(program, program.OstadId, true);
                if (!faaliatValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = $"برنامه دارای خطاهای زیر است و قابل تأیید نیست: {faaliatError}",
                        warnings = faaliatWarnings
                    });
                }

                // ============================================================
                // 5️⃣ بررسی کامل بودن برنامه (حداقل ساعت + ۵ روز)
                // ============================================================
                var completeness = await ValidateProgramCompletenessAsync(program, program.OstadId, program.CodeTerm);
                if (!completeness.IsValid)
                {
                    return BadRequest(new
                    {
                        success = false,
                        message = completeness.Message
                    });
                }

                // ============================================================
                // 6️⃣ تأیید
                // ============================================================
                program.NazarElmi = 1;
                program.TarikhElmi = DateTime.UtcNow;
                program.IsLocked = true;

                await _context.SaveChangesAsync();

                _cache.Remove($"PermittedMarkaz_{program.OstadId}_{program.CodeTerm}");

                return Ok(new
                {
                    success = true,
                    message = "برنامه با موفقیت توسط استاد تأیید شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در تأیید برنامه توسط استاد",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 7️⃣ تأیید توسط مدیر گروه
        // ============================================================
        [HttpPatch("confirm/modir/{id}")]
        public async Task<IActionResult> ConfirmByModirGrooh(int id, [FromBody] ModirGroohApproveDto dto)
        {
            try
            {
                // ۱. اطلاعات کاربر فعلی
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ۲. بررسی اینکه کاربر نقش مدیر گروه دارد (CodeRole = 3 یا 4)
                bool isOstanModir = codeRole == 3;   // مدیر گروه استان
                bool isMarkazModir = codeRole == 4;  // مدیر گروه مرکز

                if (!isOstanModir && !isMarkazModir)
                    return Forbid("شما مجوز مدیر گروهی ندارید");

                // ۳. دریافت برنامه و استاد
                var program = await _context.BarnamehHaftegiOstads
                    .Include(b => b.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه یافت نشد" });

                // ۴. دریافت گروه آموزشی استاد
                var ostadMadrak = await _context.OstadMadraks
                    .FirstOrDefaultAsync(m => m.OstadId == program.OstadId && m.PishFarz == true);

                if (ostadMadrak == null || ostadMadrak.GrooheAmoozeshiId == null)
                    return BadRequest(new { success = false, message = "رشته‌ی پیش‌فرض استاد مشخص نیست" });

                // ۵. دریافت AppUserRoleId از دیتابیس بر اساس کاربر و نقش فعلی
                var appUserRole = await _context.Set<AppUserRole>()
                    .FirstOrDefaultAsync(ur => ur.UserId == currentUser.Id && ur.RoleId == currentRole.Id);

                if (appUserRole == null)
                    return Unauthorized("نقش فعال کاربر در سیستم ثبت نشده است");

                // ۶. بررسی دسترسی به گروه آموزشی
                var hasAccessToGroohe = await _context.ModirGroohs
                    .AnyAsync(mg => mg.AppUserRoleId == appUserRole.Id
                                    && mg.GrooheAmoozeshiId == ostadMadrak.GrooheAmoozeshiId.Value
                                    && mg.Vazeeat == true);

                if (!hasAccessToGroohe)
                    return Forbid("شما به این گروه آموزشی دسترسی ندارید");

                // ۷. بررسی سطح دسترسی (استان یا مرکز)
                if (isOstanModir)
                {
                    if (currentMarkaz == null || program.Ostad.MarkazId == null)
                        return Forbid("اطلاعات مرکز استاد کامل نیست");

                    var canAccess = await _accessService.CanAccessTargetOstadAsync(
                        program.OstadId,
                        codeRole.Value,
                        currentMarkaz?.Id
                    );

                    if (!canAccess)
                        return Forbid("استاد در استان شما نیست");
                }
                else if (isMarkazModir)
                {
                    if (currentMarkaz == null || program.Ostad.MarkazId == null)
                        return Forbid("اطلاعات مرکز استاد کامل نیست");

                    if (program.Ostad.MarkazId.Value != currentMarkaz.Id)
                        return Forbid("استاد در مرکز شما نیست");
                }

                // ۸. بررسی وضعیت برنامه
                if (program.NazarElmi != 1)
                    return BadRequest(new { success = false, message = "برنامه باید ابتدا توسط استاد تأیید شود" });

                if (program.NazarModirGrooh != 0)
                    return BadRequest(new { success = false, message = "این برنامه قبلاً توسط مدیر گروه بررسی شده است" });

                // ============================================================
                // ۹. ثبت نظر مدیر گروه (تایید یا رد)
                // ============================================================
                program.NazarModirGrooh = dto.ApproveStatus;
                program.TarikhModirGrooh = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var statusText = dto.ApproveStatus == 1 ? "تایید" : "رد";
                return Ok(new
                {
                    success = true,
                    message = $"برنامه با موفقیت توسط مدیر گروه {statusText} شد"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بررسی برنامه توسط مدیر گروه",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 8️⃣ تأیید نهایی توسط معاون
        // ============================================================
        [HttpPatch("confirm/moaven/{id}")]
        public async Task<IActionResult> ConfirmByMoaven(int id, [FromBody] MoavenApproveDto dto)
        {
            try
            {
                // ۱. اطلاعات کاربر فعلی
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ۲. دریافت برنامه
                var program = await _context.BarnamehHaftegiOstads
                    .Include(b => b.Ostad)
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه یافت نشد" });

                // ۳. بررسی دسترسی به استاد
                if (!await _accessService.CanAccessTargetOstadAsync(program.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid("شما به این استاد دسترسی ندارید");

                // ۴. بررسی وضعیت برنامه
                if (program.NazarMoaven != 0)
                    return BadRequest(new { success = false, message = "این برنامه قبلاً توسط معاون بررسی شده است" });

                // ۵. اخطار در صورت عدم نظر مدیرگروه
                List<string> warnings = new();
                if (program.NazarModirGrooh == 0)
                {
                    warnings.Add("مدیر گروه هنوز نظری در مورد این برنامه ثبت نکرده است. آیا از ادامه مطمئن هستید؟");
                }
                else if (program.NazarModirGrooh == 2)
                {
                    warnings.Add("مدیر گروه این برنامه را رد کرده است. آیا می‌خواهید نظر مدیر گروه را نادیده بگیرید؟");
                }

                // ۶. ثبت نظر معاون
                program.NazarMoaven = dto.ApproveStatus;
                program.TarikhMoaven = DateTime.UtcNow;

                await _context.SaveChangesAsync();

                var statusText = dto.ApproveStatus == 1 ? "تایید" : "رد";
                return Ok(new
                {
                    success = true,
                    message = $"برنامه با موفقیت توسط معاون {statusText} شد",
                    warnings = warnings.Any() ? warnings : null,
                    hasWarnings = warnings.Any()
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در بررسی برنامه توسط معاون",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 9️⃣ ریست تأیید (بازگشت به حالت پیش‌نویس)
        // ============================================================
        [HttpPatch("reset/{id}")]
        public async Task<IActionResult> ResetConfirm(int id)
        {
            try
            {
                // ۱. اطلاعات کاربر فعلی
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ۲. بررسی دسترسی (فقط معاون یا ادمین سامانه)
                bool isMoaven = codeRole == 3 && currentRole?.Name?.Contains("معاون") == true;
                bool isAdmin = codeRole == 1;

                if (!isMoaven && !isAdmin)
                    return Forbid("شما مجوز ریست تأیید را ندارید");

                // ۳. دریافت برنامه
                var program = await _context.BarnamehHaftegiOstads
                    .FirstOrDefaultAsync(b => b.Id == id);

                if (program == null)
                    return NotFound(new { success = false, message = "برنامه یافت نشد" });

                // ۴. بررسی دسترسی به استاد (برای معاون)
                if (isMoaven && !await _accessService.CanAccessTargetOstadAsync(program.OstadId, codeRole.Value, currentMarkaz?.Id))
                    return Forbid("شما به این استاد دسترسی ندارید");

                // ۵. بررسی وضعیت (برنامه باید قفل باشد)
                if (!program.IsLocked && program.NazarElmi == 0)
                    return BadRequest(new { success = false, message = "برنامه در حالت پیش‌نویس است و نیازی به ریست ندارد" });

                // ۶. ریست همه نظرات
                program.IsLocked = false;
                program.NazarElmi = 0;
                program.NazarModirGrooh = 0;
                program.NazarMoaven = 0;
                program.TarikhElmi = null;
                program.TarikhModirGrooh = null;
                program.TarikhMoaven = null;

                await _context.SaveChangesAsync();

                // پاک کردن کش مراکز مجاز
                _cache.Remove($"PermittedMarkaz_{program.OstadId}_{program.CodeTerm}");

                return Ok(new
                {
                    success = true,
                    message = "برنامه با موفقیت به حالت پیش‌نویس بازگشت و قابل ویرایش است"
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در ریست برنامه",
                    error = ex.Message
                });
            }
        }

        [HttpPatch("bulk-lock")]
        public async Task<IActionResult> BulkLock([FromBody] BulkLockDto dto)
        {
            try
            {
                // ۱. اطلاعات کاربر فعلی
                var (currentUser, currentRole, currentMarkaz, codeRole) = await _currentUserService.GetCurrentUserInfoAsync();
                if (currentUser == null || codeRole == null)
                    return Unauthorized(new { success = false, message = "کاربر یا نقش معتبر نیست" });

                // ۲. بررسی مجوز (سیستم پویا - فقط مجوز را چک می‌کنیم)
                // مجوز مورد نیاز: BarnamehHaftegi.BulkLock
                // این مجوز توسط PermissionFilter بررسی می‌شود، پس اینجا نیازی به چک دستی نیست
                // اما اگر خواستید دستی چک کنید:
                // if (!await _permissionService.HasPermissionAsync(currentUser.Id, "BarnamehHaftegi.BulkLock"))
                //     return Forbid();

                // ۳. تعیین ترم (پیش‌فرض: ترم جاری)
                string termCode = dto.TermCode ?? null;
                if (string.IsNullOrEmpty(termCode))
                    return BadRequest(new { success = false, message = "ترم جاری در سیستم تعریف نشده است" });

                // ۴. ساخت کوئری برای پیدا کردن برنامه‌های هدف
                var query = _context.BarnamehHaftegiOstads
                    .Include(b => b.Ostad)
                        .ThenInclude(o => o.Markaz)
                    .Where(b => b.CodeTerm == termCode );

                // فیلتر بر اساس نوع همکاری (اختیاری)
                if (dto.NoeHamkari.HasValue)
                {
                    query = query.Where(b => b.Ostad.NoeHamkari == (NoeHamkariEnum)dto.NoeHamkari.Value);
                }

                // ۵. محدودیت دسترسی بر اساس سطح کاربر (استان یا مرکز)
                // سطح دسترسی از CodeRole استخراج می‌شود
                if (codeRole == 3) // سطح استان
                {
                    if (currentMarkaz == null)
                        return Forbid("اطلاعات مرکز کاربر کامل نیست");

                    var ostanCode = currentMarkaz.CodeOstan;
                    if (string.IsNullOrEmpty(ostanCode))
                        return Forbid("کد استان کاربر مشخص نیست");

                    var markazIdsInOstan = await _context.Markazes
                        .Where(m => m.CodeOstan == ostanCode && m.Vazeeyat == true)
                        .Select(m => m.Id)
                        .ToListAsync();

                    query = query.Where(b => b.Ostad.MarkazId.HasValue && markazIdsInOstan.Contains(b.Ostad.MarkazId.Value));
                }
                else if (codeRole == 4) // سطح مرکز
                {
                    if (currentMarkaz == null)
                        return Forbid("اطلاعات مرکز کاربر کامل نیست");

                    query = query.Where(b => b.Ostad.MarkazId == currentMarkaz.Id);
                }
                else if (codeRole == 1 || codeRole == 2) // ادمین سامانه یا سازمان
                {
                    // همه مراکز - بدون محدودیت
                }
                else
                {
                    return Forbid("شما دسترسی کافی برای این عملیات ندارید");
                }

                // ۶. دریافت لیست برنامه‌های هدف
                var programs = await query.ToListAsync();

                if (!programs.Any())
                    return Ok(new
                    {
                        success = true,
                        message = "هیچ برنامه‌ای با این فیلترها یافت نشد",
                        data = new { count = 0, updated = new List<int>() }
                    });

                // ۷. اعمال قفل/باز کردن
                bool isLock = dto.Action?.ToLower() == "lock";
                bool isUnlock = dto.Action?.ToLower() == "unlock";

                if (!isLock && !isUnlock)
                    return BadRequest(new { success = false, message = "عملیات نامعتبر. فقط 'lock' یا 'unlock' مجاز است" });

                var updatedIds = new List<int>();
                foreach (var program in programs)
                {
                    program.IsLocked = isLock;                   
                    updatedIds.Add(program.Id);
                }

                await _context.SaveChangesAsync();

                var actionText = isLock ? "قفل" : "باز کردن قفل";
                return Ok(new
                {
                    success = true,
                    message = $"{programs.Count} برنامه با موفقیت {actionText} شدند",
                    data = new
                    {
                        count = programs.Count,
                        updatedIds = updatedIds,
                        action = dto.Action
                    }
                });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new
                {
                    success = false,
                    message = "خطا در قفل/باز کردن گروهی برنامه‌ها",
                    error = ex.Message
                });
            }
        }

        // ============================================================
        // 🔥 متدهای کمکی نمایشی
        // ============================================================

        private string GetNazarDisplay(int? nazar)
        {
            return nazar switch
            {
                0 => "در انتظار",
                1 => "تایید ✅",
                2 => "رد ❌",
                _ => "نامشخص"
            };
        }

        private string GetDayDisplay(string? day)
        {
            return day switch
            {
                "1" => "شنبه",
                "2" => "یکشنبه",
                "3" => "دوشنبه",
                "4" => "سه‌شنبه",
                "5" => "چهارشنبه",
                "6" => "پنجشنبه",
                _ => day ?? "-"
            };
        }

        private string GetMarkazName(int? markazId, List<Markaz> allMarkaz)
        {
            if (!markazId.HasValue) return "-";
            var markaz = allMarkaz.FirstOrDefault(m => m.Id == markazId.Value);
            return markaz?.NaamMarkaz ?? "-";
        }

        private bool IsDayPermitted(BarnamehHaftegiOstad1 detail, List<PermittedMarkazInfo> permittedMarkazInfo)
        {
            var permittedMarkazIds = permittedMarkazInfo.Select(x => x.MarkazId).ToHashSet();

            // بررسی مرکز اصلی روز
            if (detail.MarkazId.HasValue && permittedMarkazIds.Contains(detail.MarkazId.Value))
                return true;

            // بررسی مراکز ساعتی
            var markazIds = new List<int?>
            {
                detail.MarkazIdA, detail.MarkazIdB, detail.MarkazIdC, detail.MarkazIdD,
                detail.MarkazIdE, detail.MarkazIdF, detail.MarkazIdG, detail.MarkazIdH
            };

            return markazIds.Any(id => id.HasValue && permittedMarkazIds.Contains(id.Value));
        }
        /// <summary>
        /// دریافت لیست فعالیت‌های فعال از سرویس کش
        /// </summary>
        private async Task<List<Faaliat>> GetActiveFaaliatsAsync()
        {
            return await _faaliatCacheService.GetAllActiveAsync();
        }

        
    }

    // ============================================================
    // 📦 DTOها
    // ============================================================

    public class BarnamehHaftegiCreateDto
    {
        [Required]
        public int OstadId { get; set; }

        [Required]
        [MaxLength(50)]
        public string CodeTerm { get; set; } = string.Empty;

        [Required]
        public List<BarnamehHaftegiDetailCreateDto> Details { get; set; } = new();
    }

    public class BarnamehHaftegiDetailCreateDto
    {
        [Required]
        [MaxLength(10)]
        public string RoozeHafteh { get; set; } = string.Empty;

        [Required]
        public int MarkazId { get; set; }  // 🔥 مرکز اصلی روز

        public int? A { get; set; }
        public int? MarkazIdA { get; set; }
        public int? B { get; set; }
        public int? MarkazIdB { get; set; }
        public int? C { get; set; }
        public int? MarkazIdC { get; set; }
        public int? D { get; set; }
        public int? MarkazIdD { get; set; }
        public int? E { get; set; }
        public int? MarkazIdE { get; set; }
        public int? F { get; set; }
        public int? MarkazIdF { get; set; }
        public int? G { get; set; }
        public int? MarkazIdG { get; set; }
        public int? H { get; set; }
        public int? MarkazIdH { get; set; }
        public bool? Jozeiat { get; set; }
    }

    public class BarnamehHaftegiUpdateDto
    {
        [Required]
        public List<BarnamehHaftegiDetailCreateDto> Details { get; set; } = new();
    }

    public class BarnamehHaftegiListDto
    {
        // اطلاعات استاد
        public int OstadId { get; set; }
        public string OstadName { get; set; } = string.Empty;
        public string OstadCode { get; set; } = string.Empty;
        public string OstadMarkaz { get; set; } = string.Empty;
        public int NoeHamkari { get; set; }
        public string? MartabeElmi { get; set; }
        public int? Maghta { get; set; }
        public string? Reshteh { get; set; }
        public int? GrooheAmoozeshiId { get; set; }

        // وضعیت برنامه
        public bool HasProgram { get; set; }
        public string ApproveStatus { get; set; } = string.Empty;  // pishnevis, tayeed_ostad, tayeed_modir, tayeed_moaven, no_program
        public string ApproveStatusDisplay { get; set; } = string.Empty;

        // اطلاعات برنامه (در صورت وجود)
        public int? ProgramId { get; set; }
        public int? NazarElmi { get; set; }
        public int? NazarModirGrooh { get; set; }
        public int? NazarMoaven { get; set; }
        public bool IsLocked { get; set; }
        public DateTime? CreatedAt { get; set; }
    }

    public class BarnamehHaftegiDetailDto
    {
        public int Id { get; set; }
        public int OstadId { get; set; }
        public string OstadName { get; set; } = string.Empty;
        public string OstadCode { get; set; } = string.Empty;
        public string OstadMarkaz { get; set; } = string.Empty;
        public string TermTitle { get; set; } = string.Empty;
        public string CodeTerm { get; set; } = string.Empty;

        public int? NazarElmi { get; set; }
        public string NazarElmiDisplay { get; set; } = string.Empty;
        public int? NazarModirGrooh { get; set; }
        public string NazarModirGroohDisplay { get; set; } = string.Empty;
        public int? NazarMoaven { get; set; }
        public string NazarMoavenDisplay { get; set; } = string.Empty;
        public bool IsLocked { get; set; }

        public string ApproveStatus { get; set; } = string.Empty;
        public string ApproveStatusDisplay { get; set; } = string.Empty;

        public DateTime? TarikhElmi { get; set; }
        public DateTime? TarikhModirGrooh { get; set; }
        public DateTime? TarikhMoaven { get; set; }

        public int TotalSessions { get; set; }
        public int RequiredSessions { get; set; }
        public int RequiredHours { get; set; }
        public bool IsComplete { get; set; }

        public List<BarnamehHaftegiDetailItemDto> Details { get; set; } = new();
    }

    public class BarnamehHaftegiDetailItemDto
    {
        public int Id { get; set; }
        public string RoozeHafteh { get; set; } = string.Empty;
        public string RoozeHaftehDisplay { get; set; } = string.Empty;

        // 🔥 فیلدهای جدید برای مرکز اصلی روز
        public int? MarkazId { get; set; }
        public string? MarkazName { get; set; }

        public int? A { get; set; }
        public int? MarkazIdA { get; set; }
        public string? MarkazNameA { get; set; }

        public int? B { get; set; }
        public int? MarkazIdB { get; set; }
        public string? MarkazNameB { get; set; }

        public int? C { get; set; }
        public int? MarkazIdC { get; set; }
        public string? MarkazNameC { get; set; }

        public int? D { get; set; }
        public int? MarkazIdD { get; set; }
        public string? MarkazNameD { get; set; }

        public int? E { get; set; }
        public int? MarkazIdE { get; set; }
        public string? MarkazNameE { get; set; }

        public int? F { get; set; }
        public int? MarkazIdF { get; set; }
        public string? MarkazNameF { get; set; }

        public int? G { get; set; }
        public int? MarkazIdG { get; set; }
        public string? MarkazNameG { get; set; }

        public int? H { get; set; }
        public int? MarkazIdH { get; set; }
        public string? MarkazNameH { get; set; }

        public bool? Jozeiat { get; set; }
        public bool IsPermittedDay { get; set; }
    }

    public class ModirGroohApproveDto
    {
        [Required]
        [Range(1, 2, ErrorMessage = "مقدار باید ۱ (تایید) یا ۲ (رد) باشد")]
        public int ApproveStatus { get; set; }
    }

    public class MoavenApproveDto
    {
        [Required]
        [Range(1, 2, ErrorMessage = "مقدار باید ۱ (تایید) یا ۲ (رد) باشد")]
        public int ApproveStatus { get; set; }
    }

    public class BulkLockDto
    {
        /// <summary>
        /// نوع همکاری استاد (اختیاری - اگر مقدار نداشته باشد، همه اساتید شامل می‌شوند)
        /// 1=هیات علمی پیام نور، 2=هیات علمی غیر پیام نور، 3=مدرس مدعو، 4=سایر
        /// </summary>
        public int? NoeHamkari { get; set; }

        /// <summary>
        /// عملیات: "lock" برای قفل کردن، "unlock" برای باز کردن قفل
        /// </summary>
        [Required]
        public string Action { get; set; } = string.Empty;

        /// <summary>
        /// کد ترم (اختیاری - اگر نباشد، ترم جاری استفاده می‌شود)
        /// </summary>
        [MaxLength(50)]
        public string? TermCode { get; set; }
    }

    public class PermittedMarkazInfo
    {
        public int MarkazId { get; set; }
        public bool IsMainMarkaz { get; set; }
        public int? MaxDays { get; set; }           // فقط برای مراکز غیراصلی
        public List<int> AllowedFaaliatIds { get; set; } = new();
        public int NoeMarkaz { get; set; }          // نوع مرکز: 1=حضوری, 2=مجازی, 3=ترکیبی
    }
}