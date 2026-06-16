using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models;
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

                // فقط نقش Admin را ایجاد کن
                string sysAdminRole = "ادمین سامانه";
                if (!await roleManager.RoleExistsAsync(sysAdminRole))
                {
                    var roleResult = await roleManager.CreateAsync(new AppRole
                    {
                        Name = sysAdminRole,
                        CodeGrooheKarbari = 1,
                        Vazeeyat = true,
                        Emza = false
                    });
                    if (!roleResult.Succeeded)
                    {
                        throw new Exception($"خطا در ایجاد نقش: {string.Join(", ", roleResult.Errors)}");
                    }
                }

                // ایجاد کاربر SysAdmin (اگر وجود نداشت)
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
                    var userResult = await userManager.CreateAsync(adminUser, "Admin@123");
                    if (!userResult.Succeeded)
                    {
                        throw new Exception($"خطا در ایجاد کاربر: {string.Join(", ", userResult.Errors)}");
                    }

                    var roleAddResult = await userManager.AddToRoleAsync(adminUser, sysAdminRole);
                    if (!roleAddResult.Succeeded)
                    {
                        throw new Exception($"خطا در افزودن نقش: {string.Join(", ", roleAddResult.Errors)}");
                    }

                    // ثبت اطلاعات در جدول MoshakhasatAdmin
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
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در SeedAsync: {ex.Message}", ex);
            }
        }
    }
}