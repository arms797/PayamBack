using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.DTOs.Edu.Dars;
using PayamBack.Models.Edu;

namespace PayamBack.Services.Edu
{
    /// <summary>
    /// پیاده‌سازی Sync گروهی درس‌ها و منابع.
    ///
    /// اصول طراحی:
    ///   1. همه‌ی مراجع یک‌بار از DB لود می‌شن (بدون Query توی حلقه).
    ///   2. از Dictionary Lookup برای تطبیق استفاده می‌شه (O(1)).
    ///   3. ChangeTracker با Batch size = 500 خالی می‌شه.
    ///   4. خطای هر رکورد جدا گرفته می‌شه و کل فایل رو متوقف نمی‌کنه.
    ///   5. منابعی که توی فایل نیستن Hard Delete می‌شن — فقط برای درس‌هایی که توی فایل حاضرن.
    ///   6. منطق ساعت‌ها: اگر SaatTeori_DB == SaatTeoriOrginal_DB → هر دو از فایل آپدیت؛
    ///      وگرنه فقط SaatTeoriOrginal از فایل.
    /// </summary>
    public class DarsImportService : IDarsImportService
    {
        private readonly AppDbContext _context;
        private const int BatchSize = 500;

        public DarsImportService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<ImportResultDto> ImportAsync(
            List<ReshtehGroup> reshtehGroups,
            CancellationToken cancellationToken = default)
        {
            var result = new ImportResultDto();

            // ============================================================
            // 0) گرفتن ترم جاری — یک بار
            // ============================================================
            var currentTerm = await _context.Terms
                .Where(t => t.Vazeeyat == true)
                .Select(t => t.CodeTerm)
                .FirstOrDefaultAsync(cancellationToken);

            // ============================================================
            // 1) بارگذاری مراجع — یک بار
            // ============================================================
            var allReshtehs = await _context.Reshtehs
                .AsNoTracking()
                .ToListAsync(cancellationToken);

            // کلید رشته: (CodeReshte, CodeMaghta, TermVorood, TermEamal)
            var reshtehLookup = allReshtehs
                .GroupBy(r => (
                    r.CodeReshte ?? "",
                    r.CodeMaghta ?? "",
                    r.TermVorood ?? "",
                    r.TermEamal ?? ""))
                .ToDictionary(g => g.Key, g => g.First());

            // ============================================================
            // 2) پردازش گروه‌به‌گروه
            // ============================================================
            int batchCounter = 0;

            foreach (var group in reshtehGroups)
            {
                // ---- 2.1) پیدا کردن رشته ----
                var reshtehKey = (
                    group.CodeReshte ?? "",
                    group.CodeMaghta ?? "",
                    group.TermVorood ?? "",
                    group.TermEamal ?? "");

                if (!reshtehLookup.TryGetValue(reshtehKey, out var reshteh))
                {
                    result.RejectedReshtehs.Add(new RejectedReshtehDto
                    {
                        ReshtehName = group.ReshtehName,
                        CodeReshte = group.CodeReshte,
                        CodeMaghta = group.CodeMaghta,
                        TermVorood = group.TermVorood,
                        TermEamal = group.TermEamal,
                        Reason = "رشته در سیستم یافت نشد"
                    });
                    continue; // skip این رشته، برو بعدی
                }

                // ---- 2.2) بارگذاری درس‌های این رشته (فقط یک بار) ----
                // برای منطق شرطی ساعت‌ها نیاز به مقادیر قبلی SaatTeori/SaatTeoriOrginal داریم
                var existingDarsOfReshteh = await _context.Dars
                    .Include(d => d.ManbaDarsList)
                    .Where(d => d.ReshtehId == reshteh.Id)
                    .ToListAsync(cancellationToken);

                var darsLookup = existingDarsOfReshteh
                    .GroupBy(d => d.CodeDars)
                    .ToDictionary(g => g.Key, g => g.First());

                // کدهای درس حاضر در این گروه از فایل (برای حذف نکردن درس‌های غایب)
                var incomingDarsCodes = group.DarsList
                    .Select(d => d.CodeDars)
                    .Where(c => !string.IsNullOrWhiteSpace(c))
                    .ToHashSet(StringComparer.OrdinalIgnoreCase);

                // ---- 2.3) پردازش هر درس ----
                foreach (var darsRow in group.DarsList)
                {
                    try
                    {
                        if (string.IsNullOrWhiteSpace(darsRow.CodeDars))
                        {
                            result.Errors.Add(new ImportErrorDto
                            {
                                CodeReshte = group.CodeReshte,
                                ErrorMessage = "کد درس خالی است",
                                Stage = "Dars"
                            });
                            result.SkippedDars++;
                            continue;
                        }

                        if (darsLookup.TryGetValue(darsRow.CodeDars, out var existingDars))
                        {
                            // --------- UPDATE ---------
                            UpdateDarsFromRow(existingDars, darsRow);
                            SyncManbas(existingDars, darsRow.ManbaList, currentTerm, result);
                            result.UpdatedDars++;
                        }
                        else
                        {
                            // --------- INSERT ---------
                            var newDars = CreateNewDars(darsRow, reshteh.Id, currentTerm);
                            SyncManbas(newDars, darsRow.ManbaList, currentTerm, result);

                            await _context.Dars.AddAsync(newDars, cancellationToken);
                            darsLookup[darsRow.CodeDars] = newDars; // برای جلوگیری از insert دوباره
                            result.InsertedDars++;
                        }

                        batchCounter++;
                        if (batchCounter >= BatchSize)
                        {
                            await SaveBatchAsync(cancellationToken);
                            batchCounter = 0;
                        }
                    }
                    catch (Exception ex)
                    {
                        result.Errors.Add(new ImportErrorDto
                        {
                            CodeReshte = group.CodeReshte,
                            CodeDars = darsRow.CodeDars,
                            ErrorMessage = ex.Message,
                            Stage = "Dars"
                        });
                        result.SkippedDars++;
                    }
                }
            }

            // ============================================================
            // 3) ذخیره‌ی نهایی
            // ============================================================
            await SaveBatchAsync(cancellationToken);

            return result;
        }

        // ============================================================
        // ساخت درس جدید از ردیف فایل
        // ============================================================
        private Dars CreateNewDars(DarsWithManba row, int reshtehId, string? currentTerm)
        {
            return new Dars
            {
                CodeDars = row.CodeDars,
                NaamDars = row.NaamDars,
                VahedTeori = row.VahedTeori,
                VahedAmali = row.VahedAmali,

                // برای درس جدید، هر دو از فایل گرفته می‌شن (یک سلول مشترک)
                SaatTeori = row.SaatTeori_File,
                SaatTeoriOrginal = row.SaatTeori_File,
                SaatAmali = row.SaatAmali_File,
                SaatAmaliOrginal = row.SaatAmali_File,

                TermAkhz = row.TermAkhz,
                NoeDars = row.NoeDars,
                NoeAzmoon = row.NoeAzmoon,
                ReshtehId = reshtehId,
                Zarfiat = row.Zarfiat,
                CreatedAt = DateTime.Now,
                UpdatedAt = DateTime.Now,
                ManbaDarsList = new List<ManbaDars>()
            };
        }

        // ============================================================
        // آپدیت درس موجود از ردیف فایل
        // ============================================================
        private void UpdateDarsFromRow(Dars dars, DarsWithManba row)
        {
            // --- فیلدهای ساده: همیشه از فایل ---
            dars.NaamDars = row.NaamDars ?? dars.NaamDars;
            dars.VahedTeori = row.VahedTeori ?? dars.VahedTeori;
            dars.VahedAmali = row.VahedAmali ?? dars.VahedAmali;
            dars.TermAkhz = row.TermAkhz ?? dars.TermAkhz;
            dars.NoeDars = row.NoeDars ?? dars.NoeDars;
            dars.NoeAzmoon = row.NoeAzmoon ?? dars.NoeAzmoon;
            dars.Zarfiat = row.Zarfiat ?? dars.Zarfiat;

            // --- منطق شرطی ساعت‌ها ---
            ApplySaatLogic(
                currentValue: dars.SaatTeori,
                currentOriginal: dars.SaatTeoriOrginal,
                fileValue: row.SaatTeori_File,
                out var newSaatTeori,
                out var newSaatTeoriOrginal);

            dars.SaatTeori = newSaatTeori;
            dars.SaatTeoriOrginal = newSaatTeoriOrginal;

            ApplySaatLogic(
                currentValue: dars.SaatAmali,
                currentOriginal: dars.SaatAmaliOrginal,
                fileValue: row.SaatAmali_File,
                out var newSaatAmali,
                out var newSaatAmaliOrginal);

            dars.SaatAmali = newSaatAmali;
            dars.SaatAmaliOrginal = newSaatAmaliOrginal;

            dars.UpdatedAt = DateTime.Now;
        }

        /// <summary>
        /// منطق شرطی ساعت‌ها:
        ///   اگر currentValue == currentOriginal  →  کاربر دست نزده  →  هر دو از فایل
        ///   اگر currentValue != currentOriginal  →  کاربر دست زده   →  فقط Orginal از فایل
        /// </summary>
        private static void ApplySaatLogic(
            int? currentValue,
            int? currentOriginal,
            int? fileValue,
            out int? newValue,
            out int? newOriginal)
        {
            // اگر فایل مقداری نداره، دست نزن
            if (!fileValue.HasValue)
            {
                newValue = currentValue;
                newOriginal = currentOriginal;
                return;
            }

            if (currentValue == currentOriginal)
            {
                // کاربر دست نزده → هر دو از فایل
                newValue = fileValue;
                newOriginal = fileValue;
            }
            else
            {
                // کاربر دست زده → فقط Orginal از فایل، Value حفظ
                newValue = currentValue;
                newOriginal = fileValue;
            }
        }

        // ============================================================
        // Sync منابع یک درس
        //   - کلید یکتایی: (DarsId, ShomareManba, CodePeyvast)
        //   - موجود در فایل + موجود در DB  → Update
        //   - موجود در فایل + غایب در DB   → Insert
        //   - غایب در فایل + موجود در DB   → Delete (Hard)
        // ============================================================
        private void SyncManbas(
            Dars dars,
            List<ManbaImportRow> incomingManbas,
            string? currentTerm,
            ImportResultDto result)
        {
            dars.ManbaDarsList ??= new List<ManbaDars>();

            // فقط منابع معتبر (حداقل یه Onvan یا ShomareManba داشته باشن)
            var validIncoming = incomingManbas
                .Where(m => !string.IsNullOrWhiteSpace(m.Onvan)
                         || !string.IsNullOrWhiteSpace(m.ShomareManba))
                .ToList();

            // کلید یکتایی
            static string MakeKey(string? shomare, string? codePeyvast)
                => $"{(shomare ?? "").Trim()}|{(codePeyvast ?? "").Trim()}";

            var incomingKeys = validIncoming
                .Select(m => MakeKey(m.ShomareManba, m.CodePeyvast))
                .ToHashSet();

            // ---- 1) Delete: منابعی که توی فایل نیستن ----
            var toDelete = dars.ManbaDarsList
                .Where(m => !incomingKeys.Contains(MakeKey(m.ShomareManba, m.CodePeyvast)))
                .ToList();

            foreach (var manba in toDelete)
            {
                _context.ManbaDars.Remove(manba);
                result.DeletedManba++;
            }

            // ---- 2) Insert / Update ----
            var existingByKey = dars.ManbaDarsList
                .Where(m => !toDelete.Contains(m))
                .GroupBy(m => MakeKey(m.ShomareManba, m.CodePeyvast))
                .ToDictionary(g => g.Key, g => g.First());

            foreach (var row in validIncoming)
            {
                var key = MakeKey(row.ShomareManba, row.CodePeyvast);

                if (existingByKey.TryGetValue(key, out var existing))
                {
                    // --------- UPDATE ---------
                    ApplyManbaUpdate(existing, row, currentTerm);
                    result.UpdatedManba++;
                }
                else
                {
                    // --------- INSERT ---------
                    var newManba = CreateNewManba(row, dars.Id, currentTerm);
                    dars.ManbaDarsList.Add(newManba);
                    result.InsertedManba++;
                }
            }
        }

        private static void ApplyManbaUpdate(ManbaDars manba, ManbaImportRow row, string? currentTerm)
        {
            // Smart Diff: فقط اگه تغییری بود، UpdatedAt/TermUpdate رو عوض کن
            bool hasChanged =
                manba.Onvan != (row.Onvan ?? string.Empty) ||
                manba.NoeManba != row.NoeManba ||
                manba.Nevisandeh != row.Nevisandeh ||
                manba.Motarjem != row.Motarjem ||
                manba.SalEnteshar != row.SalEnteshar ||
                manba.SalEntesharMiladi != row.SalEntesharMiladi ||
                manba.Shabak != row.Shabak ||
                manba.Nasher != row.Nasher ||
                manba.NobateChap != row.NobateChap ||
                manba.Vazeeyat != row.Vazeeyat ||
                manba.SharhPeyvast != row.SharhPeyvast;

            manba.Onvan = row.Onvan ?? string.Empty;
            manba.NoeManba = row.NoeManba;
            manba.Nevisandeh = row.Nevisandeh;
            manba.Motarjem = row.Motarjem;
            manba.SalEnteshar = row.SalEnteshar;
            manba.SalEntesharMiladi = row.SalEntesharMiladi;
            manba.Shabak = row.Shabak;
            manba.Nasher = row.Nasher;
            manba.NobateChap = row.NobateChap;
            manba.Vazeeyat = row.Vazeeyat;
            manba.SharhPeyvast = row.SharhPeyvast;

            if (hasChanged)
            {
                manba.TermUpdate = currentTerm;
                manba.UpdatedAt = DateTime.Now;
            }
        }

        private static ManbaDars CreateNewManba(ManbaImportRow row, int darsId, string? currentTerm)
        {
            return new ManbaDars
            {
                DarsId = darsId,
                ShomareManba = row.ShomareManba,
                NoeManba = row.NoeManba,
                Onvan = row.Onvan ?? string.Empty,
                Nevisandeh = row.Nevisandeh,
                Motarjem = row.Motarjem,
                SalEnteshar = row.SalEnteshar,
                SalEntesharMiladi = row.SalEntesharMiladi,
                Shabak = row.Shabak,
                Nasher = row.Nasher,
                NobateChap = row.NobateChap,
                Vazeeyat = row.Vazeeyat,
                CodePeyvast = row.CodePeyvast,
                SharhPeyvast = row.SharhPeyvast,
                TermUpdate = currentTerm,
                CreatedAt = DateTime.Now
            };
        }

        // ============================================================
        // Save + Clear ChangeTracker
        // ============================================================
        private async Task SaveBatchAsync(CancellationToken cancellationToken)
        {
            await _context.SaveChangesAsync(cancellationToken);
            _context.ChangeTracker.Clear();
        }
    }
}
