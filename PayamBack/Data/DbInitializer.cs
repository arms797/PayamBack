using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Identity;

namespace PayamBack
{
    public static class DbInitializer
    {
        public static async Task SeedAsync(IServiceProvider serviceProvider)
        {
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            var roleManager = scope.ServiceProvider.GetRequiredService<RoleManager<AppRole>>();
            var userManager = scope.ServiceProvider.GetRequiredService<UserManager<AppUser>>();

            try
            {
                await context.Database.EnsureCreatedAsync();

                // ============================================================
                // 6️⃣ اگر مرکزی وجود ندارد، یک مرکز پیش‌فرض ایجاد کن
                // ============================================================
                //var existsMarkaz =await context.Set<Markaz>()
                //    .AnyAsync();
                int markazId = 0;
                if (!await context.Markazes.AnyAsync())
                {
                    var markaz = new Markaz
                    {
                        CodeMarkaz = "6293",
                        NaamMarkaz = "مرکز شیراز",
                        CodeOstan = "16",
                        NaamOstan = "فارس",
                        Vazeeyat = true
                    };
                    await context.Markazes.AddAsync(markaz);
                    await context.SaveChangesAsync();
                    markazId = markaz.Id;
                }

                // ============================================================
                // 1️⃣ ایجاد نقش "ادمین سامانه"
                // ============================================================
                string adminRoleName = "ادمین سامانه";
                AppRole? adminRole = await roleManager.FindByNameAsync(adminRoleName);
                if (adminRole == null)
                {
                    adminRole = new AppRole
                    {
                        Name = adminRoleName,
                        CodeRole = 1,
                        Vazeeyat = true,
                        Emza = false
                    };
                    await roleManager.CreateAsync(adminRole);
                }

                // ============================================================
                // 2️⃣ ایجاد کاربر ادمین
                // ============================================================
                var adminEmail = "admin@payam.ac.ir";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);
                if (adminUser == null)
                {
                    // ============================================================
                    // 3️⃣ ابتدا مشخصات ادمین را ایجاد کن تا Id آن را داشته باشیم
                    // ============================================================
                    var moshakhasatAdmin = new MoshakhasatAdmin
                    {
                        CodeMelli = "1234567890",
                        Naam = "سیستم",
                        NaameKhanevadeghi = "ادمین",
                        TelefonMostaghim = "",
                        TelefonGhayreMostaghim = "",
                        TelefonDakheli = "",
                        Mobile = "",
                        Mobile2 = null,
                        Email = adminEmail,
                        Adres = "",
                        CodePosti = ""
                    };
                    await context.MoshakhasatAdmins.AddAsync(moshakhasatAdmin);
                    await context.SaveChangesAsync();  // ← برای گرفتن Id

                    // ============================================================
                    // 4️⃣ ایجاد کاربر با AdminId
                    // ============================================================
                    adminUser = new AppUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        AdminId = moshakhasatAdmin.Id,  // ← مقداردهی AdminId
                        Vazeeyat = true,
                        VazeeyatMovaghat = true
                    };
                    await userManager.CreateAsync(adminUser, "Admin@123");

                    // اضافه کردن نقش به کاربر
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);

                    // ============================================================
                    // 5️⃣ ثبت در جدول AppUserRole (برای RolePishFarz)
                    // ============================================================
                    var appUserRole = new AppUserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                        MarkazId = markazId,
                        RolePishFarz = true
                    };
                    await context.Set<AppUserRole>().AddAsync(appUserRole);

                    await context.SaveChangesAsync();
                }
                else
                {
                    // ============================================================
                    // اگر کاربر وجود دارد، بررسی کن که در AppUserRole ثبت شده است یا نه
                    // ============================================================
                    var existsInAppUserRole = await context.Set<AppUserRole>()
                        .AnyAsync(ur => ur.UserId == adminUser.Id);

                    if (!existsInAppUserRole && adminRole != null)
                    {
                        var appUserRole = new AppUserRole
                        {
                            UserId = adminUser.Id,
                            RoleId = adminRole.Id,
                            MarkazId = 1,
                            RolePishFarz = true
                        };
                        await context.Set<AppUserRole>().AddAsync(appUserRole);
                        await context.SaveChangesAsync();
                    }

                    // ============================================================
                    // بررسی کن که AdminId مقدار دارد یا نه
                    // ============================================================
                    if (adminUser.AdminId == null)
                    {
                        var moshakhasatAdmin = await context.MoshakhasatAdmins
                            .FirstOrDefaultAsync(m => m.Email == adminEmail);

                        if (moshakhasatAdmin != null)
                        {
                            adminUser.AdminId = moshakhasatAdmin.Id;
                            await userManager.UpdateAsync(adminUser);
                        }
                    }
                }
                
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در SeedAsync: {ex.Message}", ex);
            }
        }
    }
}