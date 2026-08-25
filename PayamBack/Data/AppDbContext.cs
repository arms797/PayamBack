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
        public DbSet<ElmiTerm> ElmiTerms { get; set; }
        public DbSet<UserSignature> UserSignatures { get; set; }
        public DbSet<ModirGrooh> ModirGroohs { get; set; }

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
            builder.Entity<Markaz>()
                .HasIndex(m => m.CodeMarkaz)
                .IsUnique()
                .HasDatabaseName("IX_Markaz_CodeMarkaz");

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
        }
    }
}