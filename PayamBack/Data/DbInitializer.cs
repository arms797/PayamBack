using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PayamBack.Data;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;

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
                        Vazeeyat = true,
                        Dakheli= true,
                        Level=4,
                        NoeMarkaz=1,
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
                /*
                string adminRoleName = "ادمین سامانه";
                AppRole? adminRole = await roleManager.FindByNameAsync(adminRoleName);
                if (adminRole == null)
                {
                    adminRole = new AppRole
                    {
                        Name = adminRoleName,
                        CodeRole = 1,
                        Vazeeyat = true,
                        Emza = false,
                        IsAdmin= true,
                        IsUniquePerMarkaz=false
                    };
                    await roleManager.CreateAsync(adminRole);
                }
                */
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

                    //مجوز های نقش 
                    new() { Resource = "Role", Action = "View", Name = "Role.View", Description = "مشاهده لیست نقش ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Role", Action = "Create", Name = "Role.Create", Description = "ایجاد نقش جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Role", Action = "Update", Name = "Role.Update", Description = "ویرایش نقش", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Role", Action = "Delete", Name = "Role.Delete", Description = "حذف نقش", IsActive = true, CreatedAt = DateTime.UtcNow },
                   
                    // مجوزهای تخصیص مجوز به نقش
                    new() { Resource = "RolePermission", Action = "View", Name = "RolePermission.View", Description = "مشاهده تخصیص مجوزها به نقش‌ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RolePermission", Action = "Create", Name = "RolePermission.Create", Description = "تخصیص مجوز به نقش", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RolePermission", Action = "Delete", Name = "RolePermission.Delete", Description = "حذف مجوز از نقش", IsActive = true, CreatedAt = DateTime.UtcNow },

                    // مجوزهای مدیریت اساتید (برای آینده)
                    new() { Resource = "Ostad", Action = "View", Name = "Ostad.View", Description = "مشاهده لیست اساتید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "Create", Name = "Ostad.Create", Description = "ایجاد استاد جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "Update", Name = "Ostad.Update", Description = "ویرایش استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "Delete", Name = "Ostad.Delete", Description = "حذف استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Ostad", Action = "BulkUpload", Name = "Ostad.BulkUpload", Description = "افزودن گروهی استاد", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های ادمین کنترلر
                    new() { Resource = "Admin", Action = "View", Name = "Admin.View", Description = "مشاهده لیست ادمین ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Admin", Action = "Create", Name = "Admin.Create", Description = "ایجاد ادمین جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Admin", Action = "Update", Name = "Admin.Update", Description = "ویرایش ادمین", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Admin", Action = "Delete", Name = "Admin.Delete", Description = "حذف ادمین", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های کارمند 
                    new() { Resource = "Karmand", Action = "View", Name = "Karmand.View", Description = "مشاهده لیست کارمند ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Karmand", Action = "Create", Name = "Karmand.Create", Description = "ایجاد کارمند جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Karmand", Action = "Update", Name = "Karmand.Update", Description = "ویرایش کارمند", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Karmand", Action = "Delete", Name = "Karmand.Delete", Description = "حذف کارمند", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های مرکز 
                    new() { Resource = "Markaz", Action = "View", Name = "Markaz.View", Description = "مشاهده لیست مرکز ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Markaz", Action = "Create", Name = "Markaz.Create", Description = "ایجاد مرکز جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Markaz", Action = "Update", Name = "Markaz.Update", Description = "ویرایش مرکز", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Markaz", Action = "Delete", Name = "Markaz.Delete", Description = "حذف مرکز", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های استاد مدرک 
                    new() { Resource = "OstadMadrak", Action = "View", Name = "OstadMadrak.View", Description = "مشاهده لیست مدرک های استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "OstadMadrak", Action = "Create", Name = "OstadMadrak.Create", Description = "ایجاد مدرک جدید استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "OstadMadrak", Action = "Update", Name = "OstadMadrak.Update", Description = "ویرایش مدرک استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "OstadMadrak", Action = "Delete", Name = "OstadMadrak.Delete", Description = "حذف مدرک استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "OstadMadrak", Action = "Approve", Name = "OstadMadrak.Approve", Description = "تایید مدرک استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "OstadMadrak", Action = "Unapprove", Name = "OstadMadrak.Unapprove", Description = "عدم تایید مدرک استاد", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های گروه آموزشی 
                    new() { Resource = "GrooheAmoozeshi", Action = "View", Name = "GrooheAmoozeshi.View", Description = "مشاهده لیست گروه آموزشی ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "GrooheAmoozeshi", Action = "Create", Name = "GrooheAmoozeshi.Create", Description = "ایجاد گروه آموزشی جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "GrooheAmoozeshi", Action = "Update", Name = "GrooheAmoozeshi.Update", Description = "ویرایش گروه آموزشی", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "GrooheAmoozeshi", Action = "Delete", Name = "GrooheAmoozeshi.Delete", Description = "حذف گروه آموزشی", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های رشته 
                    new() { Resource = "Reshteh", Action = "View", Name = "Reshteh.View", Description = "مشاهده لیست رشته ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Reshteh", Action = "Create", Name = "Reshteh.Create", Description = "ایجاد رشته جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Reshteh", Action = "Update", Name = "Reshteh.Update", Description = "ویرایش رشته", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Reshteh", Action = "Delete", Name = "Reshteh.Delete", Description = "حذف رشته", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های ترم 
                    new() { Resource = "Term", Action = "View", Name = "Term.View", Description = "مشاهده لیست ترم ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Create", Name = "Term.Create", Description = "ایجاد ترم جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Update", Name = "Term.Update", Description = "ویرایش ترم", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Delete", Name = "Term.Delete", Description = "حذف ترم", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های مدیرگروه 
                    new() { Resource = "ModirGrooh", Action = "View", Name = "ModirGrooh.View", Description = "مشاهده لیست مدیرگروه ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ModirGrooh", Action = "Create", Name = "ModirGrooh.Create", Description = "ایجاد مدیرگروه جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ModirGrooh", Action = "Update", Name = "ModirGrooh.Update", Description = "ویرایش مدیرگروه", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ModirGrooh", Action = "Delete", Name = "ModirGrooh.Delete", Description = "حذف مدیرگروه", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های انتصاب‌ 
                    new() { Resource = "RoleAssignment", Action = "View", Name = "RoleAssignment.View", Description = "مشاهده لیست انتصاب‌ ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RoleAssignment", Action = "Create", Name = "RoleAssignment.Create", Description = "ایجاد انتصاب‌ جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RoleAssignment", Action = "Update", Name = "RoleAssignment.Update", Description = "ویرایش انتصاب‌", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "RoleAssignment", Action = "Delete", Name = "RoleAssignment.Delete", Description = "حذف انتصاب‌", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های امضا 
                    new() { Resource = "Signature", Action = "View", Name = "Signature.View", Description = "مشاهده لیست امضا ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Signature", Action = "SaveSignature", Name = "Signature.SaveSignature", Description = "ویرایش امضا", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Signature", Action = "ChangePosition", Name = "Signature.ChangePosition", Description = "تغییر موقعیت امضا", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Signature", Action = "UnlockSignature", Name = "Signature.UnlockSignature", Description = "باز کردن قفل امضا", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Signature", Action = "Delete", Name = "Signature.Delete", Description = "حذف نقش", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Signature", Action = "ManageSignatureForReset", Name = "Signature.ManageSignatureForReset", Description = "لیست امضادارها برای ریست امضا", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های کاربر 
                    new() { Resource = "User", Action = "View", Name = "User.View", Description = "مشاهده اطلاعات کاربر", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "User", Action = "Update", Name = "User.Update", Description = "ویرایش کاربر", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های برنامه هفتگی 
                    new() { Resource = "BarnamehHaftegi", Action = "View", Name = "BarnamehHaftegi.View", Description = "مشاهده برنامه هفتگی", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "Create", Name = "BarnamehHaftegi.Create", Description = "ایجاد برنامه هفتگی جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "Update", Name = "BarnamehHaftegi.Update", Description = "ویرایش برنامه هفتگی", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "Delete", Name = "BarnamehHaftegi.Delete", Description = "حذف برنامه هفتگی", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "BulkLock", Name = "BarnamehHaftegi.BulkLock", Description = "قفل برنامه هفتگی", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "ConfirmByMoaven", Name = "BarnamehHaftegi.ConfirmByMoaven", Description = "تایید معاون", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "ConfirmByOstad", Name = "BarnamehHaftegi.ConfirmByOstad", Description = "تایید استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "ConfirmByModirGrooh", Name = "BarnamehHaftegi.ConfirmByModirGrooh", Description = "تایید مدیرگروه", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "BarnamehHaftegi", Action = "ResetConfirm", Name = "BarnamehHaftegi.ResetConfirm", Description = "ریست به پیش نویس", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های علمی ترم 
                    new() { Resource = "ElmiTerm", Action = "View", Name = "ElmiTerm.View", Description = "مشاهده لیست وضعیت ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ElmiTerm", Action = "Create", Name = "ElmiTerm.Create", Description = "ایجاد وضعیت جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ElmiTerm", Action = "Update", Name = "ElmiTerm.Update", Description = "ویرایش وضعیت", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ElmiTerm", Action = "Delete", Name = "ElmiTerm.Delete", Description = "حذف وضعیت", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ElmiTerm", Action = "Approve", Name = "ElmiTerm.Approve", Description = "تایید وضعیت", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "ElmiTerm", Action = "ResetToPending", Name = "ElmiTerm.ResetToPending", Description = "ریست وضعیت به حالت پیش نویس", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های ترم 
                    new() { Resource = "Faaliat", Action = "View", Name = "Faaliat.View", Description = "مشاهده لیست فعالیت ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Faaliat", Action = "Create", Name = "Faaliat.Create", Description = "ایجاد فعالیت جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Faaliat", Action = "Update", Name = "Faaliat.Update", Description = "ویرایش فعالیت", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Faaliat", Action = "Delete", Name = "Faaliat.Delete", Description = "حذف فعالیت", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های تدریس‌همجوار 
                    new() { Resource = "Hamjavar", Action = "View", Name = "Hamjavar.View", Description = "مشاهده لیست تدریس‌همجوار ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "Create", Name = "Hamjavar.Create", Description = "ایجاد تدریس‌همجوار جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "Update", Name = "Hamjavar.Update", Description = "ویرایش تدریس‌همجوار", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "Delete", Name = "Hamjavar.Delete", Description = "حذف تدریس‌همجوار", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "ConfirmSubmitByOstad", Name = "Hamjavar.ConfirmSubmitByOstad", Description = "تایید نهایی استاد", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "ReviewByRaeis", Name = "Hamjavar.ReviewByRaeis", Description = "بررسی رییس مرکز", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "ReviewByKhadamat", Name = "Hamjavar.ReviewByKhadamat", Description = "بررسی خدمات آموزشی استان", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Hamjavar", Action = "ReviewByMoaven", Name = "Hamjavar.ReviewByMoaven", Description = "بررسی معاون استان", IsActive = true, CreatedAt = DateTime.UtcNow },
/*
                    //مجوز های ترم 
                    new() { Resource = "Term", Action = "View", Name = "Term.View", Description = "مشاهده لیست ترم ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Create", Name = "Term.Create", Description = "ایجاد ترم جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Update", Name = "Term.Update", Description = "ویرایش ترم", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Delete", Name = "Term.Delete", Description = "حذف ترم", IsActive = true, CreatedAt = DateTime.UtcNow },

                    //مجوز های ترم 
                    new() { Resource = "Term", Action = "View", Name = "Term.View", Description = "مشاهده لیست ترم ها", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Create", Name = "Term.Create", Description = "ایجاد ترم جدید", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Update", Name = "Term.Update", Description = "ویرایش ترم", IsActive = true, CreatedAt = DateTime.UtcNow },
                    new() { Resource = "Term", Action = "Delete", Name = "Term.Delete", Description = "حذف ترم", IsActive = true, CreatedAt = DateTime.UtcNow },
*/


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
                // 04 ایجاد منوهای پیش‌فرض
                // ============================================================
                var defaultMenus = new List<Menu>
                {
                    new() { Title = "داشبورد", Icon = "bi-grid-1x2-fill", Path = "/dashboard", PermissionName = null, Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "مدیریت سیستم", Icon = "bi-gear-fill", Path = null, PermissionName = null, Order = 2, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "مجوزها", ParentId = 2, Icon = "bi-shield-lock-fill", Path = "/permissions", PermissionName = "Permission.View", Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "منوها", ParentId = 2, Icon = "bi-list-ul", Path = "/menus", PermissionName = "Menu.View", Order = 2, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "تخصیص مجوز", ParentId = 2, Icon = "bi-person-badge-fill", Path = "/role-permissions", PermissionName = "RolePermission.View", Order = 3, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "نقش ها", ParentId = 2, Icon = "bi-person-badge-fill", Path = "/roles", PermissionName = "Role.View", Order = 4, Vazeeat = true, CreatedAt = DateTime.UtcNow },                    
                    new() { Title = "مدیریت کاربران",  Icon = "bi-people-fill",  Order = 3, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "کاربران", ParentId = 7, Icon = "bi-person-badge-fill", Path = "/personel", PermissionName = "Karmand.View", Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "اساتید", ParentId = 7, Icon = "bi-person-fill", Path = "/ostad", PermissionName = "Ostad.View", Order = 2, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "فعالیت های علمی", ParentId = 2, Icon = "bi-book-fill", Path = "/faaliat", PermissionName = "Faaliat.View", Order = 5, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "برنامه های اساتید",  Icon = "bi-calendar-event-fill",  Order = 4, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "وضعیت ترمی هیات علمی", ParentId = 11, Icon = "bi-gear-wide-connected", Path = "/elmi-term", PermissionName = "ElmiTerm.View", Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "درخواست تدریس در سایر مراکز", ParentId = 11, Icon = "bi-calendar-event-fill", Path = "/tadris-hamjavar-list", PermissionName = "Hamjavar.View", Order = 2, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "پروفایل کاربر", Icon = "bi-gear-fill", Order = 20, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "امضا شخصی", ParentId = 14, Icon = "bi-shield-lock-fill", Path = "/profile/signature", PermissionName = "Signature.View", Order = 1, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "مدیریت امضای کاربران", ParentId = 7, Icon = "bi-gear-fill", Path = "signatures/manage", PermissionName = "Signature.UnlockSignature", Order = 3, Vazeeat = true, CreatedAt = DateTime.UtcNow },
                    new() { Title = "برنامه هفتگی", ParentId = 11, Icon = "bi-calendar-event-fill", Path = "/barnameh-haftegi-list", PermissionName = "BarnamehHaftegi.View", Order = 5, Vazeeat = true, CreatedAt = DateTime.UtcNow },

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

                if (!context.FaaliatGroups.Any())
                {
                    var fg = new List<FaaliatGroup>
                    {
                        new() { Title="فعالیت آموزشی",IsActive=true,MinSaatDarHafteh=0,MaxSaatDarHafteh=20,
                                MinSaatDarEdari=0,MaxSaatDarEdari=20,MinDayDarHafteh=0,MaxDayDarHafteh=7},
                        new() { Title="فعالیت پژوهشی",IsActive=true,MinSaatDarHafteh=0,MaxSaatDarHafteh=10,
                                MinSaatDarEdari=0,MaxSaatDarEdari=6,MinDayDarHafteh=0,MaxDayDarHafteh=7},
                        new() { Title="فعالیت اجرایی",IsActive=true,MinSaatDarHafteh=12,MaxSaatDarHafteh=40,
                                MinSaatDarEdari=12,MaxSaatDarEdari=40,MinDayDarHafteh=2,MaxDayDarHafteh=7},
                    };
                    await context.FaaliatGroups.AddRangeAsync(fg);
                    await context.SaveChangesAsync();
                }

                if (!context.Faaliats.Any())
                {
                    var f = new List<Faaliat>
                    {
                        new() { Onvan="تدریس حضوری",NoeAnjam=1,MinSaatDarHafteh=0,MaxSaatDarHafteh=20,
                                MinSaatDarEdari=0,MaxSaatDarEdari=20,MinDayDarHafteh=0,MaxDayDarHafteh=7,
                                IsMadove=true,Vazeeat=true,FaaliatGroupId=1,Color="#8ff095"},
                        new() { Onvan="تدریس مجازی",NoeAnjam=2,MinSaatDarHafteh=0,MaxSaatDarHafteh=20,
                                MinSaatDarEdari=0,MaxSaatDarEdari=20,MinDayDarHafteh=0,MaxDayDarHafteh=7,
                                IsMadove=true,Vazeeat=true,FaaliatGroupId=1,Color="#e1fe4d"},
                        new() { Onvan="پژوهشی",NoeAnjam=1,MinSaatDarHafteh=0,MaxSaatDarHafteh=10,
                                MinSaatDarEdari=0,MaxSaatDarEdari=6,MinDayDarHafteh=0,MaxDayDarHafteh=7,
                                IsMadove=false,Vazeeat=true,FaaliatGroupId=2,Color="#7180cc"},
                        new() { Onvan="فعالیت اجرایی",NoeAnjam=1,MinSaatDarHafteh=0,MaxSaatDarHafteh=40,
                                MinSaatDarEdari=0,MaxSaatDarEdari=40,MinDayDarHafteh=0,MaxDayDarHafteh=7,
                                IsMadove=false,Vazeeat=true,FaaliatGroupId=3,Color="#fb4dfe"},
                        new() { Onvan="مشاوره دانشجویی",NoeAnjam=1,MinSaatDarHafteh=4,MaxSaatDarHafteh=40,
                                MinSaatDarEdari=4,MaxSaatDarEdari=40,MinDayDarHafteh=2,MaxDayDarHafteh=7,
                                IsMadove=false,Vazeeat=true,FaaliatGroupId=3,Color="#eead81"},
                        new() { Onvan="فعالیت فرهنگی",NoeAnjam=1,MinSaatDarHafteh=0,MaxSaatDarHafteh=40,
                                MinSaatDarEdari=0,MaxSaatDarEdari=40,MinDayDarHafteh=0,MaxDayDarHafteh=7,
                                IsMadove=false,Vazeeat=true,FaaliatGroupId=3,Color="#e3cee3"},
                    };
                    await context.Faaliats.AddRangeAsync(f);
                    await context.SaveChangesAsync();
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
                if (!context.SaatBargozariKelashas.Any())
                {
                    var saats = new List<SaatBargozariKelasha>
                    {
                        new() { OnvanSaat = "ساعت اول", CodeSaat = "A", SaatShoroo = "08", SaatPayan = "10", Hozoori = true,Majazi=true },
                        new() { OnvanSaat = "ساعت دوم", CodeSaat = "B", SaatShoroo = "10", SaatPayan = "12", Hozoori = true,Majazi=true },
                        new() { OnvanSaat = "ساعت سوم", CodeSaat = "C", SaatShoroo = "12", SaatPayan = "14", Hozoori = true,Majazi=true },
                        new() { OnvanSaat = "ساعت چهارم", CodeSaat = "D", SaatShoroo = "14", SaatPayan = "16", Hozoori = true,Majazi=true },
                        new() { OnvanSaat = "ساعت پنجم", CodeSaat = "E", SaatShoroo = "16", SaatPayan = "18", Hozoori = true,Majazi=true },
                        new() { OnvanSaat = "ساعت ششم", CodeSaat = "F", SaatShoroo = "18", SaatPayan = "20", Hozoori = false,Majazi=false },
                        new() { OnvanSaat = "ساعت هفتم", CodeSaat = "G", SaatShoroo = "20", SaatPayan = "22", Hozoori = false,Majazi=false },
                        new() { OnvanSaat = "ساعت هشتم", CodeSaat = "H", SaatShoroo = "22", SaatPayan = "24", Hozoori = false,Majazi=false },
                    };
                    await context.SaatBargozariKelashas.AddRangeAsync(saats);
                    await context.SaveChangesAsync();
                }
                if (!context.Terms.Any())
                {
                    var t = new List<Term>
                    {
                        new(){CodeTerm="4042",OnvanTerm="نیمسال دوم 1405-1404",TermJariShoroo=new DateOnly(2026,1,1),
                            TermJariPayan=new DateOnly(2026,6,21),TarikheDastrasi=new DateOnly(2025,12,6),
                            TarikheEraeeDars=new DateOnly(2025,12,10),TarikhePayanDars=new DateOnly(2026,3,6),
                            TarikheShorooClass=new DateOnly(2026,2,1),TarikhePayanClass=new DateOnly(2026,5,24),
                            TarikheShorooMojavezMarakez=new DateOnly(2025,12,7),TarikhePayanMojavezMarakez=new DateOnly(2026,1,30),
                            Vazeeyat=false,IsHaftegiRequired=true
                        },
                        new(){CodeTerm="4043",OnvanTerm="نیمسال تابستان 1405-1404",TermJariShoroo=new DateOnly(2026,6,22),
                            TermJariPayan=new DateOnly(2026,9,14),TarikheDastrasi=new DateOnly(2026,6,22),
                            TarikheEraeeDars=new DateOnly(2026,6,22),TarikhePayanDars=new DateOnly(2026,7,22),
                            TarikheShorooClass=new DateOnly(2026,7,11),TarikhePayanClass=new DateOnly(2026,9,1),
                            TarikheShorooMojavezMarakez=new DateOnly(2026,6,22),TarikhePayanMojavezMarakez=new DateOnly(2026,7,11),
                            Vazeeyat=true,IsHaftegiRequired=false
                        },
                        new(){CodeTerm="4051",OnvanTerm="نیمسال اول 1406-1405",TermJariShoroo=new DateOnly(2026,9,23),
                            TermJariPayan=new DateOnly(2027,1,20),TarikheDastrasi=new DateOnly(2026,9,1),
                            TarikheEraeeDars=new DateOnly(2026,9,6),TarikhePayanDars=new DateOnly(2026,10,22),
                            TarikheShorooClass=new DateOnly(2026,9,23),TarikhePayanClass=new DateOnly(2026,12,21),
                            TarikheShorooMojavezMarakez=new DateOnly(2026,9,1),TarikhePayanMojavezMarakez=new DateOnly(2026,9,23),
                            Vazeeyat=true,IsHaftegiRequired=true
                        },
                    };
                    await context.Terms.AddRangeAsync(t);
                    await context.SaveChangesAsync();
                }
                
                if (!context.Roles.Any())
                {
                    var role = new List<AppRole>
                    {
                        new(){Name="ادمین سامانه",CodeRole=1,Vazeeyat=true,Emza=true,IsAdmin=true,IsUniquePerMarkaz=false},
                        new(){Name="ادمین دانشگاه",CodeRole=2,Vazeeyat=true,Emza=true,IsAdmin=true,IsUniquePerMarkaz=false},
                        new(){Name="رییس دانشگاه",CodeRole=2,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=true},
                        new(){Name="ادمین استان",CodeRole=3,Vazeeyat=true,Emza=true,IsAdmin=true,IsUniquePerMarkaz=false},
                        new(){Name="رییس استان",CodeRole=3,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=true},
                        new(){Name="معاون آموزشی استان",CodeRole=3,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=true},
                        new(){Name="مدیر خدمات آموزشی استان",CodeRole=3,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=true},
                        new(){Name="مدیر گروه",CodeRole=3,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=false},
                        new(){Name="ادمین مرکز",CodeRole=4,Vazeeyat=true,Emza=true,IsAdmin=true,IsUniquePerMarkaz=false},
                        new(){Name="رییس مرکز",CodeRole=4,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=true},
                        new(){Name="برنامه‌ریزی",CodeRole=4,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=false},
                        new(){Name="مسئول برنامه‌ریزی",CodeRole=4,Vazeeyat=true,Emza=false,IsAdmin=false,IsUniquePerMarkaz=false},
                        new(){Name="استاد",CodeRole=4,Vazeeyat=true,Emza=true,IsAdmin=false,IsUniquePerMarkaz=false},
                        new(){Name="دانشجو",CodeRole=4,Vazeeyat=true,Emza=false,IsAdmin=false,IsUniquePerMarkaz=false},
                    };
                    await context.Roles.AddRangeAsync(role);
                    await context.SaveChangesAsync();
                    //adminRole = role.Where(r => r.Name == "ادمین سامانه").FirstOrDefault();
                }

                // ============================================================
                // 05 تخصیص همه مجوزها به نقش "ادمین سامانه"
                // ============================================================
                var adminRole = await context.Set<AppRole>().Where(r => r.Name == "ادمین سامانه").FirstOrDefaultAsync();
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
                    await userManager.AddToRoleAsync(adminUser, adminRole.Name);                   
                    

                    // ثبت در AppUserRole
                    var appUserRole = new AppUserRole
                    {
                        UserId = adminUser.Id,
                        RoleId = adminRole.Id,
                        MarkazId = markazId,
                        RolePishFarz = true
                    };
                    await context.Set<AppUserRole>().AddAsync(appUserRole);
                    //await context.AppUserRoles.AddAsync(appUserRole);
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
                        //await context.UserRoles.AddAsync(appUserRole);
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
                
            }
            catch (Exception ex)
            {
                throw new Exception($"خطا در SeedAsync: {ex.Message}", ex);
            }
        }
    }
}