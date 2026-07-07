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
                    adminUser = new AppUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        Vazeeyat = true,
                        VazeeyatMovaghat = true
                    };
                    await userManager.CreateAsync(adminUser, "Admin@123");

                    // اضافه کردن نقش به کاربر
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);

                    // ============================================================
                    // 3️⃣ 🔥 ثبت در جدول AppUserRole (برای RolePishFarz)
                    // ============================================================
                    var appUserRole = new AppUserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                        MarkazId = 1,  // شناسه مرکز پیش‌فرض
                        RolePishFarz = true
                    };
                    await context.Set<AppUserRole>().AddAsync(appUserRole);

                    // ============================================================
                    // 4️⃣ ثبت در جدول MoshakhasatAdmin
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
                }

                // ============================================================
                // 5️⃣ اگر مرکزی وجود ندارد، یک مرکز پیش‌فرض ایجاد کن
                // ============================================================
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
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در SeedAsync: {ex.Message}", ex);
            }
        }
    }
}