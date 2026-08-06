using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayamBack.Models.Audit;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;

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
            builder.Entity<BarnamehHaftegiOstad>()
                .HasIndex(b => new { b.OstadId, b.CodeTerm, b.MarkazId, b.RoozeHafteh })
                .IsUnique()
                .HasDatabaseName("IX_BarnamehHaftegiOstad_CodeOstad_CodeTerm_MarkazId_RoozeHafteh");

            builder.Entity<BarnamehHaftegiOstad>()
                .HasOne(b => b.Ostad)
                .WithMany(o => o.BarnamehHaftegiOstads)
                .HasForeignKey(b => b.OstadId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<BarnamehHaftegiOstad>()
                .HasOne(b => b.Markaz)
                .WithMany(m => m.BarnamehHaftegiOstads)
                .HasForeignKey(b => b.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

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

            // ============================================================
            // 🔥 تنظیمات مدل‌های جدید
            // ============================================================

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

                entity.HasIndex(e => e.AkharinTaghaza)
                    .HasDatabaseName("IX_Hamjavar_AkharinTaghaza");
            });

            // ======== Hamjavar1 ========
            builder.Entity<Hamjavar1>(entity =>
            {
                entity.HasKey(e => e.Id);

                entity.HasOne(e => e.Hamjavar)
                    .WithOne()
                    .HasForeignKey<Hamjavar1>(e => e.HamjavarId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.UserSabtKonandeh)
                    .WithMany()
                    .HasForeignKey(e => e.UserIdSabtKonandeh)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.RoleSabtKonandeh)
                    .WithMany()
                    .HasForeignKey(e => e.RoleIdSabtKonandeh)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Markaz)
                    .WithMany()
                    .HasForeignKey(e => e.MarkazId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.Faaliat)
                    .WithMany()
                    .HasForeignKey(e => e.FaaliatId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => e.HamjavarId)
                    .IsUnique()
                    .HasDatabaseName("IX_Hamjavar1_HamjavarId");

                entity.HasIndex(e => e.FaaliatId)
                    .HasDatabaseName("IX_Hamjavar1_FaaliatId");
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

                entity.HasOne(e => e.Term)
                    .WithMany()
                    .HasForeignKey(e => e.TermCode)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.UserSabtKonandeh)
                    .WithMany()
                    .HasForeignKey(e => e.UserIdSabtKonandeh)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.RoleSabtKonandeh)
                    .WithMany()
                    .HasForeignKey(e => e.RoleIdSabtKonandeh)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasOne(e => e.ApprovedByUser)
                    .WithMany()
                    .HasForeignKey(e => e.ApprovedByUserId)
                    .OnDelete(DeleteBehavior.NoAction);

                entity.HasIndex(e => new { e.UserId, e.TermCode })
                    .IsUnique()
                    .HasDatabaseName("IX_ElmiTerm_UserId_TermCode");

                entity.HasIndex(e => e.ApproveStatus)
                    .HasDatabaseName("IX_ElmiTerm_Approve");
            });
        }
    }
}