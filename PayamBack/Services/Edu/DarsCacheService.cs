using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Dars;

namespace PayamBack.Services.Edu
{
    // ============================================================
    // 1️⃣ Implementation
    // ============================================================
    public class DarsCacheService : IDarsCacheService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DarsCacheService> _logger;

        private volatile IReadOnlyList<DarsCacheDto> _darsList = Array.Empty<DarsCacheDto>();
        private volatile Dictionary<int, DarsCacheDto> _darsById = new();
        private volatile bool _isInitialized = false;

        private readonly SemaphoreSlim _rebuildLock = new(1, 1);

        public DateTime? LastRebuiltAt { get; private set; }

        public DarsCacheService(
            IServiceScopeFactory scopeFactory,
            ILogger<DarsCacheService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task<IReadOnlyList<DarsCacheDto>> GetAllAsync(CancellationToken cancellationToken = default)
        {
            if (!_isInitialized)
            {
                await RebuildAsync(cancellationToken);
            }
            return _darsList;
        }

        public DarsCacheDto? GetById(int id)
        {
            if (!_isInitialized)
                return null;

            return _darsById.TryGetValue(id, out var dto) ? dto : null;
        }
        public async Task RebuildAsync(CancellationToken cancellationToken = default)
        {
            await _rebuildLock.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("شروع بازسازی کش دروس...");
                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var list = await context.Dars
                    .AsNoTracking()
                    .Select(d => new DarsCacheDto
                    {
                        Id = d.Id,
                        CodeDars = d.CodeDars,
                        NaamDars = d.NaamDars,
                        VahedTeori = d.VahedTeori,
                        VahedAmali = d.VahedAmali,
                        SaatTeoriOrginal = d.SaatTeoriOrginal,
                        SaatAmaliOrginal = d.SaatAmaliOrginal,
                        SaatTeori = d.SaatTeori,
                        SaatAmali = d.SaatAmali,
                        TermAkhz = d.TermAkhz,
                        NoeDars = d.NoeDars,
                        NoeAzmoon = d.NoeAzmoon,

                        ReshtehId = d.ReshtehId,
                        ReshtehName = d.Reshteh != null ? d.Reshteh.OnvanReshte : null,
                        CodeReshteh = d.Reshteh != null ? d.Reshteh.CodeReshte : null,       // 🔥 جدید
                        TermVorood = d.Reshteh != null ? d.Reshteh.TermVorood : null,        // 🔥 جدید
                        TermEamal = d.Reshteh != null ? d.Reshteh.TermEamal : null,          // 🔥 جدید
                        CodeMaghta = d.Reshteh != null ? d.Reshteh.CodeMaghta : null,
                        Maghta = null,

                        GrooheAmoozeshiId = d.Reshteh != null ? d.Reshteh.GrooheAmoozeshiId : null,
                        CodeGrooheAmoozeshi = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.CodeGrooheAmoozeshi
                            : null,
                        GrooheAoozeshiName = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.OnvanGrooheAmoozeshi
                            : null,

                        CodeDaneshkadeh = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.CodeDaneshkade
                            : null,
                        DaneshkadehName = null,

                        Zarfiat = d.Zarfiat,

                        ManbaList = d.ManbaDarsList
                            .OrderBy(m => m.Id)
                            .Select(m => new ManbaCacheDto
                            {
                                Id = m.Id,
                                ShomareManba = m.ShomareManba,
                                Onvan = m.Onvan ?? string.Empty,
                                CodePeyvast = m.CodePeyvast
                            })
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                // ============================================================
                // 🔥 سورت نهایی بر اساس ۵ معیار
                // ============================================================
                list = list
                    .OrderBy(d => d.CodeReshteh ?? "")                              // ۱. کد رشته (صعودی)
                    .ThenByDescending(d => ParseIntSafe(d.TermVorood))               // ۲. ترم ورود (نزولی)
                    .ThenByDescending(d => ParseIntSafe(d.TermEamal))                // ۳. ترم اعمال (نزولی)
                    .ThenBy(d => d.CodeMaghta ?? "")                                 // ۴. کد مقطع (صعودی)
                    .ThenBy(d => d.CodeDars)                                         // ۵. کد درس (صعودی)
                    .ToList();

                _darsById = list.ToDictionary(d => d.Id);
                _darsList = list;
                _isInitialized = true;
                LastRebuiltAt = DateTime.Now;

                sw.Stop();
                _logger.LogInformation("کش دروس بازسازی شد: {Count} درس در {Elapsed}ms",
                    list.Count, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بازسازی کش دروس");
                throw;
            }
            finally
            {
                _rebuildLock.Release();
            }
        }

        // 🔥 متد کمکی برای تبدیل امن string به int
        private static int ParseIntSafe(string? value)
        {
            if (string.IsNullOrEmpty(value)) return 0;
            return int.TryParse(value, out var result) ? result : 0;
        }

        /*
        public async Task RebuildAsync(CancellationToken cancellationToken = default)
        {
            await _rebuildLock.WaitAsync(cancellationToken);
            try
            {
                _logger.LogInformation("شروع بازسازی کش دروس...");
                var sw = System.Diagnostics.Stopwatch.StartNew();

                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var list = await context.Dars
                    .AsNoTracking()
                    .OrderBy(d => d.CodeDars)
                    .Select(d => new DarsCacheDto
                    {
                        Id = d.Id,
                        CodeDars = d.CodeDars,
                        NaamDars = d.NaamDars,
                        VahedTeori = d.VahedTeori,
                        VahedAmali = d.VahedAmali,
                        SaatTeoriOrginal = d.SaatTeoriOrginal,
                        SaatAmaliOrginal = d.SaatAmaliOrginal,
                        SaatTeori = d.SaatTeori,
                        SaatAmali = d.SaatAmali,
                        TermAkhz = d.TermAkhz,
                        NoeDars = d.NoeDars,
                        NoeAzmoon = d.NoeAzmoon,

                        ReshtehId = d.ReshtehId,
                        ReshtehName = d.Reshteh != null ? d.Reshteh.OnvanReshte : null,
                        CodeMaghta = d.Reshteh != null ? d.Reshteh.CodeMaghta : null,
                        Maghta = null,

                        GrooheAmoozeshiId = d.Reshteh != null ? d.Reshteh.GrooheAmoozeshiId : null,
                        CodeGrooheAmoozeshi = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.CodeGrooheAmoozeshi
                            : null,
                        GrooheAoozeshiName = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.OnvanGrooheAmoozeshi
                            : null,

                        CodeDaneshkadeh = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.CodeDaneshkade
                            : null,
                        DaneshkadehName = null,

                        Zarfiat = d.Zarfiat,

                        ManbaList = d.ManbaDarsList
                            .OrderBy(m => m.Id)
                            .Select(m => new ManbaCacheDto
                            {
                                Id = m.Id,
                                ShomareManba = m.ShomareManba,
                                Onvan = m.Onvan ?? string.Empty,
                                CodePeyvast = m.CodePeyvast
                            })
                            .ToList()
                    })
                    .ToListAsync(cancellationToken);

                _darsById = list.ToDictionary(d => d.Id);
                _darsList = list;
                _isInitialized = true;
                LastRebuiltAt = DateTime.Now;

                sw.Stop();
                _logger.LogInformation("کش دروس بازسازی شد: {Count} درس در {Elapsed}ms",
                    list.Count, sw.ElapsedMilliseconds);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در بازسازی کش دروس");
                throw;
            }
            finally
            {
                _rebuildLock.Release();
            }
        }
        */

        // ============================================================
        // 🔥 Upsert یک درس — فقط یک رکورد از DB خونده می‌شه
        // ============================================================
        public async Task UpsertOneAsync(int darsId, CancellationToken cancellationToken = default)
        {
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // 🔥 فقط همون یک درس + منابعهاش
                var dto = await context.Dars
                    .AsNoTracking()
                    .Where(d => d.Id == darsId)
                    .Select(d => new DarsCacheDto
                    {
                        Id = d.Id,
                        CodeDars = d.CodeDars,
                        NaamDars = d.NaamDars,
                        VahedTeori = d.VahedTeori,
                        VahedAmali = d.VahedAmali,
                        SaatTeoriOrginal = d.SaatTeoriOrginal,
                        SaatAmaliOrginal = d.SaatAmaliOrginal,
                        SaatTeori = d.SaatTeori,
                        SaatAmali = d.SaatAmali,
                        TermAkhz = d.TermAkhz,
                        NoeDars = d.NoeDars,
                        NoeAzmoon = d.NoeAzmoon,

                        ReshtehId = d.ReshtehId,
                        ReshtehName = d.Reshteh != null ? d.Reshteh.OnvanReshte : null,
                        CodeReshteh = d.Reshteh != null ? d.Reshteh.CodeReshte : null,
                        TermVorood = d.Reshteh != null ? d.Reshteh.TermVorood : null,
                        TermEamal = d.Reshteh != null ? d.Reshteh.TermEamal : null,
                        CodeMaghta = d.Reshteh != null ? d.Reshteh.CodeMaghta : null,
                        Maghta = null,

                        GrooheAmoozeshiId = d.Reshteh != null ? d.Reshteh.GrooheAmoozeshiId : null,
                        CodeGrooheAmoozeshi = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.CodeGrooheAmoozeshi
                            : null,
                        GrooheAoozeshiName = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.OnvanGrooheAmoozeshi
                            : null,

                        CodeDaneshkadeh = d.Reshteh != null && d.Reshteh.GrooheAmoozeshi != null
                            ? d.Reshteh.GrooheAmoozeshi.CodeDaneshkade
                            : null,
                        DaneshkadehName = null,

                        Zarfiat = d.Zarfiat,

                        ManbaList = d.ManbaDarsList
                            .OrderBy(m => m.Id)
                            .Select(m => new ManbaCacheDto
                            {
                                Id = m.Id,
                                ShomareManba = m.ShomareManba,
                                Onvan = m.Onvan ?? string.Empty,
                                CodePeyvast = m.CodePeyvast
                            })
                            .ToList()
                    })
                    .FirstOrDefaultAsync(cancellationToken);

                if (dto == null)
                {
                    // 🔥 رکورد توی DB پیدا نشد → از کش حذف کن
                    RemoveOne(darsId);
                    return;
                }

                // 🔥 آپدیت اتمیک کش
                await _rebuildLock.WaitAsync(cancellationToken);
                try
                {
                    // ۱. آپدیت Dictionary
                    var newDict = new Dictionary<int, DarsCacheDto>(_darsById);
                    newDict[darsId] = dto;
                    _darsById = newDict;

                    // ۲. آپدیت List (حذف قدیمی + اضافه جدید)
                    var newList = _darsList.Where(d => d.Id != darsId).ToList();
                    newList.Add(dto);

                    // ۳. سورت (همون ۵ معیار)
                    newList = newList
                        .OrderBy(d => d.CodeReshteh ?? "")
                        .ThenByDescending(d => ParseIntSafe(d.TermVorood))
                        .ThenByDescending(d => ParseIntSafe(d.TermEamal))
                        .ThenBy(d => d.CodeMaghta ?? "")
                        .ThenBy(d => d.CodeDars)
                        .ToList();

                    _darsList = newList;

                    _logger.LogInformation("درس {DarsId} در کش آپدیت شد", darsId);
                }
                finally
                {
                    _rebuildLock.Release();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در آپدیت درس {DarsId} در کش", darsId);
                throw;
            }
        }

        // ============================================================
        // 🔥 حذف یک درس از کش
        // ============================================================
        public void RemoveOne(int darsId)
        {
            if (!_isInitialized)
            {
                // کش هنوز ساخته نشده → کاری نکن
                return;
            }

            _rebuildLock.Wait();
            try
            {
                // ۱. حذف از Dictionary
                var newDict = new Dictionary<int, DarsCacheDto>(_darsById);
                newDict.Remove(darsId);
                _darsById = newDict;

                // ۲. حذف از List
                _darsList = _darsList.Where(d => d.Id != darsId).ToList();

                _logger.LogInformation("درس {DarsId} از کش حذف شد", darsId);
            }
            finally
            {
                _rebuildLock.Release();
            }
        }
    }

    // ============================================================
    // 2️⃣ Warm-up Hosted Service
    // ============================================================
    public class DarsCacheWarmupHostedService : IHostedService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<DarsCacheWarmupHostedService> _logger;

        public DarsCacheWarmupHostedService(
            IServiceScopeFactory scopeFactory,
            ILogger<DarsCacheWarmupHostedService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Warm-up کش دروس...");
            try
            {
                using var scope = _scopeFactory.CreateScope();
                var cacheService = scope.ServiceProvider.GetRequiredService<IDarsCacheService>();
                await cacheService.RebuildAsync(cancellationToken);
                _logger.LogInformation("Warm-up کش دروس انجام شد.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "خطا در Warm-up کش دروس — برنامه ادامه می‌دهد.");
            }
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;
    }

    // ============================================================
    // 3️⃣ Extension برای ثبت در DI
    // ============================================================
    public static class DarsCacheServiceExtensions
    {
        public static IServiceCollection AddDarsCacheServices(this IServiceCollection services)
        {
            services.AddSingleton<IDarsCacheService, DarsCacheService>();
            services.AddHostedService<DarsCacheWarmupHostedService>();
            return services;
        }
    }


}