using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
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
                // 1️⃣ ایجاد مرکز پیش‌فرض (اگر وجود نداشت)
                // ============================================================
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
                else
                {
                    markazId = await context.Markazes
                        .Select(m => m.Id)
                        .FirstOrDefaultAsync();
                }

                // ============================================================
                // 2️⃣ ایجاد نقش "ادمین سامانه"
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
                // 3️⃣ ایجاد مجوزهای پیش‌فرض (Permission)
                // ============================================================
                var defaultPermissions = new List<Permission>
                {
                    // مجوزهای مدیریت مجوزها
                    new() { Resource = "Permission", Action = "View", Name = "Permission.View", Description = "مشاهده لیست مجوزها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Permission", Action = "Create", Name = "Permission.Create", Description = "ایجاد مجوز جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Permission", Action = "Update", Name = "Permission.Update", Description = "ویرایش مجوز", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Permission", Action = "Delete", Name = "Permission.Delete", Description = "حذف مجوز", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // مجوزهای مدیریت منوها
                    new() { Resource = "Menu", Action = "View", Name = "Menu.View", Description = "مشاهده لیست منوها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Menu", Action = "Create", Name = "Menu.Create", Description = "ایجاد منوی جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Menu", Action = "Update", Name = "Menu.Update", Description = "ویرایش منو", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Menu", Action = "Delete", Name = "Menu.Delete", Description = "حذف منو", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // مجوزهای تخصیص مجوز به نقش
                    new() { Resource = "RolePermission", Action = "View", Name = "RolePermission.View", Description = "مشاهده تخصیص مجوزها به نقش‌ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RolePermission", Action = "Create", Name = "RolePermission.Create", Description = "تخصیص مجوز به نقش", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RolePermission", Action = "Delete", Name = "RolePermission.Delete", Description = "حذف مجوز از نقش", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // مجوزهای مدیریت اساتید (برای آینده)
                    new() { Resource = "Ostad", Action = "View", Name = "Ostad.View", Description = "مشاهده لیست اساتید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "Create", Name = "Ostad.Create", Description = "ایجاد استاد جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "Update", Name = "Ostad.Update", Description = "ویرایش استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "Delete", Name = "Ostad.Delete", Description = "حذف استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                };

                foreach (var permission in defaultPermissions)
                {
                    var exists = await context.Permissions
                        .AnyAsync(p => p.Name == permission.Name);

                    if (!exists)
                    {
                        await context.Permissions.AddAsync(permission);
                    }
                }
                await context.SaveChangesAsync();

                // ============================================================
                // 4️⃣ تخصیص همه مجوزها به نقش "ادمین سامانه"
                // ============================================================
                var allPermissions = await context.Permissions.ToListAsync();
                foreach (var permission in allPermissions)
                {
                    var exists = await context.RolePermissions
                        .AnyAsync(rp => rp.RoleId == adminRole.Id && rp.PermissionId == permission.Id);

                    if (!exists)
                    {
                        await context.RolePermissions.AddAsync(new RolePermission
                        {
                            RoleId = adminRole.Id,
                            PermissionId = permission.Id,
                            Vazeeat = true
                        });
                    }
                }
                await context.SaveChangesAsync();

                // ============================================================
                // 5️⃣ ایجاد منوهای پیش‌فرض
                // ============================================================
                var defaultMenus = new List<Menu>
                {
                    new() { Title = "داشبورد", Icon = "bi-grid-1x2-fill", Path = "/dashboard", PermissionName = null, Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "مدیریت سیستم", Icon = "bi-gear-fill", Path = null, PermissionName = null, Order = 2, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "مجوزها", ParentId = 2, Icon = "bi-shield-lock-fill", Path = "/permissions", PermissionName = "Permission.View", Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "منوها", ParentId = 2, Icon = "bi-list-ul", Path = "/menus", PermissionName = "Menu.View", Order = 2, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "تخصیص مجوز", ParentId = 2, Icon = "bi-person-badge-fill", Path = "/role-permissions", PermissionName = "RolePermission.View", Order = 3, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                };

                foreach (var menu in defaultMenus)
                {
                    var exists = await context.Menus
                        .AnyAsync(m => m.Title == menu.Title && m.ParentId == menu.ParentId);

                    if (!exists)
                    {
                        await context.Menus.AddAsync(menu);
                    }
                }
                await context.SaveChangesAsync();

                // ============================================================
                // 6️⃣ ایجاد کاربر ادمین (اگر وجود نداشت)
                // ============================================================
                var adminEmail = "admin@payam.ac.ir";
                var adminUser = await userManager.FindByEmailAsync(adminEmail);

                if (adminUser == null)
                {
                    // ایجاد مشخصات ادمین
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

                    // ایجاد کاربر
                    adminUser = new AppUser
                    {
                        UserName = "admin",
                        Email = adminEmail,
                        AdminId = moshakhasatAdmin.Id,
                        Vazeeyat = true,
                        VazeeyatMovaghat = true
                    };
                    await userManager.CreateAsync(adminUser, "Admin@123");
                    await userManager.AddToRoleAsync(adminUser, adminRoleName);

                    // ثبت در AppUserRole
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
                    // اگر کاربر وجود دارد، بررسی و تکمیل اطلاعات
                    var existsInAppUserRole = await context.Set<AppUserRole>()
                        .AnyAsync(ur => ur.UserId == adminUser.Id);

                    if (!existsInAppUserRole && adminRole != null)
                    {
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
                if (!context.WeekDays.Any())
                {
                    var days = new List<WeekDay>
                    {
                        new() { Code = 1, Title = "شنبه", IsActive = true, Order = 1, IsHoliday = false },
                        new() { Code = 2, Title = "یکشنبه", IsActive = true, Order = 2, IsHoliday = false },
                        new() { Code = 3, Title = "دوشنبه", IsActive = true, Order = 3, IsHoliday = false },
                        new() { Code = 4, Title = "سه‌شنبه", IsActive = true, Order = 4, IsHoliday = false },
                        new() { Code = 5, Title = "چهارشنبه", IsActive = true, Order = 5, IsHoliday = false },
                        new() { Code = 6, Title = "پنجشنبه", IsActive = true, Order = 6, IsHoliday = false },
                        new() { Code = 7, Title = "جمعه", IsActive = false, Order = 7, IsHoliday = true }
                    };
                    await context.WeekDays.AddRangeAsync(days);
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