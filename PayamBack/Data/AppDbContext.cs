using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayamBack.Models.Audit;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;
using System.Security.Cryptography.Xml;

namespace PayamBack.Data
{
    public class AppDbContext
    : IdentityDbContext<AppUser, AppRole, int,
                        IdentityUserClaim<int>,
                        AppUserRole,
                        IdentityUserLogin<int>,
                        IdentityRoleClaim<int>,
                        IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public DbSet<Markaz> Markazes { get; set; }
        public DbSet<Ostad> Ostads { get; set; }
        public DbSet<Daneshjoo> Daneshjoos { get; set; }
        public DbSet<Karmand> Karmands { get; set; }
        public DbSet<MoshakhasatAdmin> MoshakhasatAdmins { get; set; }
        public DbSet<GrooheAmoozeshi> GrooheAmoozeshis { get; set; }
        public DbSet<Reshteh> Reshtehs { get; set; }
        public DbSet<BarnamehHaftegiOstad> BarnamehHaftegiOstads { get; set; }
        public DbSet<BarnamehHaftegiOstad1> BarnamehHaftegiOstad1s { get; set; }
        public DbSet<BarnamehTermiOstad> BarnamehTermiOstads { get; set; }
        public DbSet<SaatBargozariKelasha> SaatBargozariKelashas { get; set; }
        public DbSet<TaghvimTermi> TaghvimTermis { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<Sabeghe> Sabeghes { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Menu> Menus { get; set; }
        public DbSet<OstadMadrak> OstadMadraks { get; set; }

        // ============================================================
        // 🔥 DbSetهای جدید
        // ============================================================
        public DbSet<Hamjavar> Hamjavars { get; set; }
        public DbSet<Hamjavar1> Hamjavar1s { get; set; }
        public DbSet<Faaliat> Faaliats { get; set; }
        public DbSet<FaaliatGroup> FaaliatGroups { get; set; }
        public DbSet<ElmiTerm> ElmiTerms { get; set; }
        public DbSet<UserSignature> UserSignatures { get; set; }
        public DbSet<ModirGrooh> ModirGroohs { get; set; }
        public DbSet<WeekDay> WeekDays { get; set; }
        public DbSet<HaftegiException> HaftegiExceptions {  get; set; }
        public DbSet<SakhtemanKelass> SakhtemanKelasses { get; set; }
        public DbSet<Dars> Dars { get; set; }
        public DbSet<DarsEraeh> DarsEraehs { get; set; }
        public DbSet<DarsEraehOstad> DarsEraehOstads { get; set; }


        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ======== AppUser ========
            builder.Entity<AppUser>(entity =>
            {
                entity.ToTable("AspNetUsers");

                entity.HasOne(e => e.Karmand)
                    .WithOne()
                    .HasForeignKey<AppUser>(e => e.KarmandId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Ostad)
                    .WithOne()
                    .HasForeignKey<AppUser>(e => e.OstadId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Daneshjoo)
                    .WithOne()
                    .HasForeignKey<AppUser>(e => e.DaneshjooId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.MoshakhasatAdmin)
                    .WithOne()
                    .HasForeignKey<AppUser>(e => e.AdminId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ======== AppRole ========
            builder.Entity<AppRole>(entity =>
            {
                entity.ToTable("AspNetRoles");
            });

            // ======== AppRole ========
            builder.Entity<AppRole>()
                .ToTable("AspNetRoles");

            // ======== AppUserRole ========
            builder.Entity<AppUserRole>(entity =>
            {
                entity.ToTable("AspNetUserRoles");

                entity.HasKey(e => e.Id);

                entity.HasIndex(e => new { e.UserId, e.RoleId, e.MarkazId })
                    .IsUnique()
                    .HasDatabaseName("IX_AppUserRole_UserId_RoleId_MarkazId");

                entity.HasIndex(e => e.ParentUserRoleId)
                    .HasDatabaseName("IX_AppUserRole_ParentUserRoleId");

                entity.HasOne(e => e.User)
                    .WithMany(u => u.AppUserRoles)
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Role)
                    .WithMany(r => r.AppUserRoles)
                    .HasForeignKey(e => e.RoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Markaz)
                    .WithMany(m => m.AppUserRoles)
                    .HasForeignKey(e => e.MarkazId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ParentUserRole)
                    .WithMany(e => e.ChildUserRoles)
                    .HasForeignKey(e => e.ParentUserRoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.RoleId)
                    .HasDatabaseName("IX_AppUserRole_RoleId");

                entity.HasIndex(e => e.UserId)
                    .HasDatabaseName("IX_AppUserRole_UserId");
            });

            // ======== Markaz ========
            /*builder.Entity<Markaz>()
                .HasIndex(m => m.CodeMarkaz)
                .IsUnique()
                .HasDatabaseName("IX_Markaz_CodeMarkaz");
            */

            // ======== Ostad ========
            builder.Entity<Ostad>()
                .HasIndex(o => o.CodeOstadi)
                .IsUnique()
                .HasDatabaseName("IX_Ostad_CodeOstadi");

            builder.Entity<Ostad>()
                .HasIndex(o => o.MarkazId)
                .HasDatabaseName("IX_Ostad_MarkazId");

            builder.Entity<Ostad>()
                .HasOne(o => o.Markaz)
                .WithMany(m => m.Ostads)
                .HasForeignKey(o => o.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Ostad>()
                .HasOne(o => o.MarkazAsli)
                .WithMany()
                .HasForeignKey(o => o.MarkazAsliId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== OstadMadrak ========
            builder.Entity<OstadMadrak>(entity =>
            {
                entity.HasIndex(om => new { om.OstadId, om.PishFarz })
                    .HasDatabaseName("IX_OstadMadrak_OstadId_PishFarz");

                entity.HasOne(om => om.CreatedByUser)
                    .WithMany()
                    .HasForeignKey(om => om.CreatedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(om => om.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(om => om.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ======== BarnamehHaftegiOstad ========
            builder.Entity<BarnamehHaftegiOstad>(entity =>
            {
                // کلید اصلی
                entity.HasKey(b => b.Id);

                // ارتباط با استاد
                entity.HasOne(b => b.Ostad)
                    .WithMany(o => o.BarnamehHaftegiOstads)
                    .HasForeignKey(b => b.OstadId)
                    .OnDelete(DeleteBehavior.NoAction);

                // ارتباط با ترم
                entity.HasOne(b => b.Term)
                    .WithMany()
                    .HasForeignKey(b => b.CodeTerm)
                    .OnDelete(DeleteBehavior.NoAction);

                // ارتباط با کاربر مدیرگروه
                entity.HasOne(b => b.AppUserModirGrooh)
                    .WithMany()
                    .HasForeignKey(b => b.UserIdModirGrooh)
                    .OnDelete(DeleteBehavior.NoAction);
                //ارتباط با رییس مرکز
                entity.HasOne(b => b.AppUserRaeisMarkaz)
                    .WithMany()
                    .HasForeignKey(b => b.UserIdRaeisMarkaz)
                    .OnDelete(DeleteBehavior.NoAction);

                // ارتباط با کاربر معاون
                entity.HasOne(b => b.AppUserMoaven)
                    .WithMany()
                    .HasForeignKey(b => b.UserIdMoaven)
                    .OnDelete(DeleteBehavior.NoAction);

                // حذف ایندکس قبلی (اگر وجود داشت) و تنظیم ایندکس جدید
                // ایندکس منحصربه‌فرد برای استاد و ترم (هر استاد فقط یک برنامه هفتگی برای هر ترم)
                entity.HasIndex(b => new { b.OstadId, b.CodeTerm })
                    .IsUnique()
                    .HasDatabaseName("IX_BarnamehHaftegiOstad_OstadId_CodeTerm");

                // ایندکس برای جستجوی سریع‌تر بر اساس وضعیت‌ها
                entity.HasIndex(b => b.NazarModirGrooh)
                    .HasDatabaseName("IX_BarnamehHaftegiOstad_NazarModirGrooh");

                entity.HasIndex(b => b.NazarMoaven)
                    .HasDatabaseName("IX_BarnamehHaftegiOstad_NazarMoaven");
            });

            // ======== BarnamehHaftegiOstad1 ========
            builder.Entity<BarnamehHaftegiOstad1>(entity =>
            {
                entity.HasKey(b1 => b1.Id);

                // 🔥 ارتباط با برنامه هفتگی اصلی (یک به چند)
                entity.HasOne(b1 => b1.BarnamehHaftegiOstad)
                    .WithMany(b => b.BarnamehHaftegiOstad1s)  // ← Navigation Property در مدل اصلی
                    .HasForeignKey(b1 => b1.BarnamehHaftegiOstadId)
                    .OnDelete(DeleteBehavior.Cascade);  // با حذف برنامه اصلی، جزئیات نیز حذف شوند

                // ❌ Navigation Properties مربوط به Markaz و Faaliat کامنت شده‌اند
                // پس نیازی به تعریف FK برای آنها نیست

                // ایندکس‌ها
                entity.HasIndex(b1 => new { b1.BarnamehHaftegiOstadId, b1.RoozeHafteh })
                    .HasDatabaseName("IX_BarnamehHaftegiOstad1_OstadId_RoozeHafteh");

                entity.HasIndex(b1 => b1.MarkazId)
                    .HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazId");

                // ایندکس‌های جداگانه برای هر مرکز ساعت (اختیاری، برای سرعت جستجو)
                entity.HasIndex(b1 => b1.MarkazIdA).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdA");
                entity.HasIndex(b1 => b1.MarkazIdB).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdB");
                entity.HasIndex(b1 => b1.MarkazIdC).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdC");
                entity.HasIndex(b1 => b1.MarkazIdD).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdD");
                entity.HasIndex(b1 => b1.MarkazIdE).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdE");
                entity.HasIndex(b1 => b1.MarkazIdF).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdF");
                entity.HasIndex(b1 => b1.MarkazIdG).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdG");
                entity.HasIndex(b1 => b1.MarkazIdH).HasDatabaseName("IX_BarnamehHaftegiOstad1_MarkazIdH");
            });


            // ======== BarnamehTermiOstad ========
            builder.Entity<BarnamehTermiOstad>()
                .HasIndex(b => new { b.OstadId, b.CodeTerm, b.MarkazId, b.Tarikh })
                .IsUnique()
                .HasDatabaseName("IX_BarnamehTermiOstad_CodeOstad_CodeTerm_MarkazId_Tarikh");

            builder.Entity<BarnamehTermiOstad>()
                .HasOne(b => b.Ostad)
                .WithMany(o => o.BarnamehTermiOstads)
                .HasForeignKey(b => b.OstadId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BarnamehTermiOstad>()
                .HasOne(b => b.Markaz)
                .WithMany(m => m.BarnamehTermiOstads)
                .HasForeignKey(b => b.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Karmand ========
            builder.Entity<Karmand>()
                .HasOne(k => k.Markaz)
                .WithMany(m => m.Karmands)
                .HasForeignKey(k => k.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Karmand>()
                .HasOne(k => k.MarkazAsli)
                .WithMany()
                .HasForeignKey(k => k.MarkazAsliId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Daneshjoo ========
            builder.Entity<Daneshjoo>()
                .HasOne(d => d.Markaz)
                .WithMany(m => m.Daneshjoos)
                .HasForeignKey(d => d.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Daneshjoo>()
                .HasOne(d => d.MarkazAzmoon)
                .WithMany()
                .HasForeignKey(d => d.MarkazAzmoonId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Daneshjoo>()
                .HasOne(d => d.MarkazTermi)
                .WithMany()
                .HasForeignKey(d => d.MarkazTermiId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Daneshjoo>()
                .HasOne(d => d.Reshteh)
                .WithMany(r => r.Daneshjoos)
                .HasForeignKey(d => d.ReshtehId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Reshteh ========
            builder.Entity<Reshteh>()
                .HasOne(r => r.GrooheAmoozeshi)
                .WithMany(g => g.Reshtehs)
                .HasForeignKey(r => r.GrooheAmoozeshiId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Sabeghe ========
            builder.Entity<Sabeghe>()
                .HasOne(s => s.User)
                .WithMany(u => u.Sabeghes)
                .HasForeignKey(s => s.UserId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Permission ========
            builder.Entity<Permission>()
                .HasIndex(p => p.Name)
                .IsUnique()
                .HasDatabaseName("IX_Permission_Name");

            // ======== RolePermission ========
            builder.Entity<RolePermission>()
                .HasIndex(rp => new { rp.RoleId, rp.PermissionId })
                .IsUnique()
                .HasDatabaseName("IX_RolePermission_RoleId_PermissionId");

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Role)
                .WithMany()
                .HasForeignKey(rp => rp.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RolePermission>()
                .HasOne(rp => rp.Permission)
                .WithMany(p => p.RolePermissions)
                .HasForeignKey(rp => rp.PermissionId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Menu ========
            builder.Entity<Menu>()
                .HasOne(m => m.Parent)
                .WithMany(m => m.Children)
                .HasForeignKey(m => m.ParentId)
                .OnDelete(DeleteBehavior.NoAction);

            // ======== Hamjavar ========
            builder.Entity<Hamjavar>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Ostad)
                    .WithMany()
                    .HasForeignKey(e => e.OstadId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Term)
                    .WithMany()
                    .HasForeignKey(e => e.TermCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.OstadId)
                    .HasDatabaseName("IX_Hamjavar_OstadId");

                entity.HasIndex(e => e.TermCode)
                    .HasDatabaseName("IX_Hamjavar_TermCode");

                
            });

            // ======== Hamjavar1 ========
            builder.Entity<Hamjavar1>(entity =>
            {
                entity.HasKey(e => e.Id);

                // 🔥 اصلاح: One-to-Many (یک Hamjavar می‌تواند چندین Hamjavar1 داشته باشد)
                entity.HasOne(e => e.Hamjavar)
                    .WithMany(e => e.Hamjavar1s)  // ← WithMany با اشاره به مجموعه Hamjavar1s
                    .HasForeignKey(e => e.HamjavarId)
                    .OnDelete(DeleteBehavior.Cascade);  // ← با حذف Hamjavar، Hamjavar1 ها هم حذف شوند

                entity.HasOne(e => e.UserSabtKonandeh)
                    .WithMany()
                    .HasForeignKey(e => e.UserIdSabtKonandeh)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Markaz)
                    .WithMany()
                    .HasForeignKey(e => e.MarkazId)
                    .OnDelete(DeleteBehavior.NoAction);

                // 🔥 ایندکس معمولی برای سرعت جستجو
                entity.HasIndex(e => e.HamjavarId)
                    .HasDatabaseName("IX_Hamjavar1_HamjavarId");
            });

            // ======== Faaliat ========
            builder.Entity<Faaliat>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Onvan)
                    .IsUnique()
                    .HasDatabaseName("IX_Faaliat_Onvan");

                entity.HasIndex(e => e.Vazeeat)
                    .HasDatabaseName("IX_Faaliat_Vazeeat");

                entity.HasOne(e => e.FaaliatGroup)
                    .WithMany(g => g.Faaliats)  // ← اصلاح: WithMany با اشاره به مجموعه Faaliats در گروه
                    .HasForeignKey(e => e.FaaliatGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ======== FaaliatGroup ========
            builder.Entity<FaaliatGroup>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasIndex(e => e.Title)
                    .IsUnique()
                    .HasDatabaseName("IX_FaaliatGroup_Title");

                entity.HasIndex(e => e.IsActive)
                    .HasDatabaseName("IX_FaaliatGroup_IsActive");

                // رابطه‌ی معکوس (اختیاری است، چون قبلاً در Faaliat تعریف شده)
                // اما برای شفافیت بهتر است اینجا هم ذکر شود:
                entity.HasMany(e => e.Faaliats)
                    .WithOne(e => e.FaaliatGroup)
                    .HasForeignKey(e => e.FaaliatGroupId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ======== ElmiTerm ========
            builder.Entity<ElmiTerm>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);              

                entity.HasOne(e => e.UserSabtKonandeh)
                    .WithMany()
                    .HasForeignKey(e => e.UserIdSabtKonandeh)
                    .OnDelete(DeleteBehavior.NoAction);               

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);                

                entity.HasIndex(e => e.ApproveStatus)
                    .HasDatabaseName("IX_ElmiTerm_Approve");
            });

            // ======== UserSignature ========
            builder.Entity<UserSignature>(entity =>
            {
                entity.HasKey(e => e.Id);

                // ✅ رابطه One-to-One
                entity.HasOne(e => e.User)
                    .WithOne()  // ← بدون Navigation Property در سمت User
                    .HasForeignKey<UserSignature>(e => e.UserId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ======== ModirGrooh ========
            builder.Entity<ModirGrooh>(entity =>
            {
                entity.ToTable("ModirGrooh");

                entity.HasIndex(mg => new { mg.AppUserRoleId, mg.GrooheAmoozeshiId })
                    .IsUnique()
                    .HasDatabaseName("IX_ModirGrooh_AppUserRole_Groohe");

                entity.HasIndex(mg => mg.GrooheAmoozeshiId)
                    .HasDatabaseName("IX_ModirGrooh_GrooheId");

                entity.HasIndex(mg => mg.Vazeeat)
                    .HasDatabaseName("IX_ModirGrooh_Vazeeat");

                entity.HasOne(mg => mg.AppUserRole)
                    .WithMany()
                    .HasForeignKey(mg => mg.AppUserRoleId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(mg => mg.GrooheAmoozeshi)
                    .WithMany()
                    .HasForeignKey(mg => mg.GrooheAmoozeshiId)
                    .OnDelete(DeleteBehavior.NoAction);
            });

            // ======== SakhtemanKelass ========
            builder.Entity<SakhtemanKelass>(entity =>
            {
                entity.ToTable("SakhtemanKelass");

                // روابط
                entity.HasOne(e => e.Markaz)
                    .WithMany(m => m.SakhtemanKelasses)
                    .HasForeignKey(e => e.MarkazId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ایندکس‌ها (برای کوئری‌های سریع)
                entity.HasIndex(e => e.MarkazId)
                    .HasDatabaseName("IX_SakhtemanKelass_MarkazId");

                entity.HasIndex(e => new { e.MarkazId, e.CodeSakhteman })
                    .HasDatabaseName("IX_SakhtemanKelass_Markaz_CodeSakhteman");

                entity.HasIndex(e => new { e.MarkazId, e.CodeSakhteman, e.CodeClass })
                    .IsUnique()
                    .HasDatabaseName("IX_SakhtemanKelass_Markaz_Sakhteman_Class_Unique");

                entity.HasIndex(e => e.Vazeeyat)
                    .HasDatabaseName("IX_SakhtemanKelass_Vazeeyat");
            });

            // ======== Dars ========
            builder.Entity<Dars>(entity =>
            {
                entity.ToTable("Dars");

                // روابط
                entity.HasOne(e => e.Reshteh)
                    .WithMany(r => r.DarsList)
                    .HasForeignKey(e => e.ReshtehId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ============================================================
                // ایندکس‌ها
                // ============================================================

                // 🔥 یکتا: ترکیب کد درس + رشته
                // یعنی یه کد درس توی یه رشته فقط یه بار می‌تونه ثبت بشه
                entity.HasIndex(e => new { e.CodeDars, e.ReshtehId })
                    .IsUnique()
                    .HasDatabaseName("IX_Dars_CodeDars_ReshtehId_Unique");

                entity.HasIndex(e => e.ReshtehId)
                    .HasDatabaseName("IX_Dars_ReshtehId");

                entity.HasIndex(e => e.NaamDars)
                    .HasDatabaseName("IX_Dars_NaamDars");

                entity.HasIndex(e => e.TermAkhz)
                    .HasDatabaseName("IX_Dars_TermAkhz");

                entity.HasIndex(e => new { e.ReshtehId, e.TermAkhz })
                    .HasDatabaseName("IX_Dars_Reshteh_Term");
            });

            // ======== ManbaDars ========
            builder.Entity<ManbaDars>(entity =>
            {
                entity.ToTable("ManbaDars");

                // رابطه با Dars
                entity.HasOne(e => e.Dars)
                    .WithMany(d => d.ManbaDarsList)
                    .HasForeignKey(e => e.DarsId)
                    .OnDelete(DeleteBehavior.Cascade);
                // ⚠️ Cascade چون اگه درس حذف بشه، منابعش هم حذف می‌شن

                // ایندکس‌ها
                entity.HasIndex(e => e.DarsId)
                    .HasDatabaseName("IX_ManbaDars_DarsId");

                entity.HasIndex(e => e.Onvan)
                    .HasDatabaseName("IX_ManbaDars_Onvan");

                entity.HasIndex(e => new { e.DarsId, e.ShomareManba })
                    .HasDatabaseName("IX_ManbaDars_Dars_Shomare");

                entity.HasIndex(e => e.Vazeeyat)
                    .HasDatabaseName("IX_ManbaDars_Vazeeyat");
            });

            // ======== DarsEraeh ========
            builder.Entity<DarsEraeh>(entity =>
            {
                entity.ToTable("DarsEraeh");

                // ============================================================
                // روابط (FKها)
                // ============================================================
                entity.HasOne(e => e.Markaz)
                    .WithMany()
                    .HasForeignKey(e => e.MarkazId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Reshteh)
                    .WithMany()
                    .HasForeignKey(e => e.ReshtehId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.Dars)
                    .WithMany()
                    .HasForeignKey(e => e.DarsId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DarsEraehAsli)
                    .WithMany(e => e.ZirMajmooeh)
                    .HasForeignKey(e => e.ErtebatDehiId)
                    .OnDelete(DeleteBehavior.Restrict);

                // ============================================================
                // 🔥 ایندکس‌ها - بر اساس الگوهای کوئری
                // ============================================================

                // ۱. ترم + مرکز (پرکاربردترین)
                entity.HasIndex(e => new { e.CodeTerm, e.MarkazId })
                    .HasDatabaseName("IX_DarsEraeh_Term_Markaz");

                // ۲. ترم + رشته (گزارش دانشجو)
                entity.HasIndex(e => new { e.CodeTerm, e.ReshtehId })
                    .HasDatabaseName("IX_DarsEraeh_Term_Reshteh");

                // ۳. ترم + مرکز + رشته (برنامه‌ریزی ترکیبی)
                entity.HasIndex(e => new { e.CodeTerm, e.MarkazId, e.ReshtehId })
                    .HasDatabaseName("IX_DarsEraeh_Term_Markaz_Reshteh");

                // ۴. ترم + درس
                entity.HasIndex(e => new { e.CodeTerm, e.DarsId })
                    .HasDatabaseName("IX_DarsEraeh_Term_Dars");

                // ۵. ارتباط خودارجاعی
                entity.HasIndex(e => e.ErtebatDehiId)
                    .HasDatabaseName("IX_DarsEraeh_ErtebatDehiId");

                // ۶. ترم + وضعیت برنامه‌ریزی
                entity.HasIndex(e => new { e.CodeTerm, e.BarnamehRizi })
                    .HasDatabaseName("IX_DarsEraeh_Term_BarnamehRizi");

                // ۷. گروه یکتا
                entity.HasIndex(e => new { e.CodeTerm, e.MarkazId, e.DarsId, e.Grooh })
                    .IsUnique()
                    .HasDatabaseName("IX_DarsEraeh_Unique_Grooh");

                // ۸. ترم + مرکز + وضعیت
                entity.HasIndex(e => new { e.CodeTerm, e.MarkazId, e.VazeeyatDars })
                    .HasDatabaseName("IX_DarsEraeh_Term_Markaz_Vazeeyat");

                // ------------------------------------------------------------
                // 🔥 ایندکس: جستجو بر اساس کد درس (پرکاربرد)
                // ------------------------------------------------------------
                entity.HasIndex(e => new { e.CodeTerm, e.CodeDars })
                    .HasDatabaseName("IX_DarsEraeh_Term_CodeDars");

                entity.HasIndex(e => new { e.CodeTerm, e.CodeDars, e.Grooh })
                    .HasDatabaseName("IX_DarsEraeh_Term_CodeDars_Grooh");
            });

            // ======== OstadDars ========
            builder.Entity<DarsEraehOstad>(entity =>
            {
                entity.ToTable("DarsEraehOstad");

                // ============================================================
                // روابط
                // ============================================================
                entity.HasOne(e => e.Ostad)
                    .WithMany()
                    .HasForeignKey(e => e.OstadId)
                    .OnDelete(DeleteBehavior.Restrict);

                entity.HasOne(e => e.DarsEraeh)
                    .WithMany(e => e.Ostads)              // 🔥 این خط تغییر کرد
                    .HasForeignKey(e => e.DarsEraehId)
                    .OnDelete(DeleteBehavior.Cascade);

                // ============================================================
                // ایندکس‌ها
                // ============================================================
                entity.HasIndex(e => e.DarsEraehId)
                    .HasDatabaseName("IX_OstadDars_DarsEraehId");

                entity.HasIndex(e => e.OstadId)
                    .HasDatabaseName("IX_OstadDars_OstadId");

                entity.HasIndex(e => new { e.DarsEraehId, e.OstadId })
                    .IsUnique()
                    .HasDatabaseName("IX_OstadDars_Unique_Dars_Ostad");

                entity.HasIndex(e => new { e.DarsEraehId, e.Asli })
                    .HasDatabaseName("IX_OstadDars_Dars_Asli");
            });

        }

        /*
        public override int SaveChanges()
        {
            UpdateOnvanTerm();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            UpdateOnvanTerm();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void UpdateOnvanTerm()
        {
            var entries = ChangeTracker.Entries<Term>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in entries)
            {
                if (entry.Entity.Nimsal != null || entry.Entity.SalTahsili != null)
                {
                    entry.Entity.OnvanTerm = $"{entry.Entity.Nimsal} {entry.Entity.SalTahsili}".Trim();
                }
            }
        }
        */
    }
}