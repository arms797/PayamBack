using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayamBack.Models.Audit;
using PayamBack.Models.Core;
using PayamBack.Models.Edu;
using PayamBack.Models.Identity;
using PayamBack.Models.Schedule;

namespace PayamBack.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int>
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
        //public DbSet<Emkanat> Emkanats { get; set; }
        //public DbSet<RoleEmkanat> RoleEmkanats { get; set; }
        public DbSet<BarnamehHaftegiOstad> BarnamehHaftegiOstads { get; set; }
        public DbSet<BarnamehTermiOstad> BarnamehTermiOstads { get; set; }
        public DbSet<SaatBargozariKelasha> SaatBargozariKelashas { get; set; }
        public DbSet<TaghvimTermi> TaghvimTermis { get; set; }
        public DbSet<Term> Terms { get; set; }
        public DbSet<Sabeghe> Sabeghes { get; set; }
        public DbSet<Permission> Permissions { get; set; }
        public DbSet<RolePermission> RolePermissions { get; set; }
        public DbSet<Menu> Menus { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // ======== AppUserRole ========
            builder.Entity<AppUserRole>()
                .HasIndex(ur => new { ur.UserId, ur.RoleId, ur.MarkazId })
                .IsUnique()
                .HasDatabaseName("IX_AppUserRole_UserId_RoleId_MarkazId");

            builder.Entity<AppUserRole>()
                .HasOne(ur => ur.Markaz)
                .WithMany(m => m.AppUserRoles)
                .HasForeignKey(ur => ur.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

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
                .HasOne(o => o.Markaz)
                .WithMany(m => m.Ostads)
                .HasForeignKey(o => o.MarkazId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<Ostad>()
                .HasOne(o => o.MarkazAsli)
                .WithMany()
                .HasForeignKey(o => o.MarkazAsliId)
                .OnDelete(DeleteBehavior.NoAction);

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


            /*
            // ======== Emkanat ========
            builder.Entity<Emkanat>()
                .HasIndex(e => e.Code)
                .IsUnique()
                .HasDatabaseName("IX_Emkanat_Code");

            // ======== RoleEmkanat ========
            builder.Entity<RoleEmkanat>()
                .HasIndex(re => new { re.RoleId, re.EmkanatId })
                .IsUnique()
                .HasDatabaseName("IX_RoleEmkanat_RoleId_EmkanatId");

            builder.Entity<RoleEmkanat>()
                .HasOne(re => re.Role)
                .WithMany()
                .HasForeignKey(re => re.RoleId)
                .OnDelete(DeleteBehavior.NoAction);

            builder.Entity<RoleEmkanat>()
                .HasOne(re => re.Emkanat)
                .WithMany()
                .HasForeignKey(re => re.EmkanatId)
                .OnDelete(DeleteBehavior.NoAction);
            */
        }
    }
}