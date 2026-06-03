using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using PayamBack.Models;

namespace PayamBack.Data
{
    public class AppDbContext : IdentityDbContext<AppUser, AppRole, int,
    IdentityUserClaim<int>, AppUserRole, IdentityUserLogin<int>,
    IdentityRoleClaim<int>, IdentityUserToken<int>>
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        public DbSet<Emkanat> Emkanats { get; set; }
        public DbSet<Daneshjoo> Daneshjoos { get; set; }
        public DbSet<GrooheAmoozeshi> GrooheAmoozeshis { get; set; }
        public DbSet<Karmand> Karmands { get; set; }
        public DbSet<Markaz> Markazs { get; set; }
        public DbSet<MoshakhasatAdmin> MoshakhasatAdmins { get; set; }
        public DbSet<Ostad> Ostads { get; set; }
        public DbSet<Reshteh> Reshtehs { get; set; }
        public DbSet<RoleEmkanat> RoleEmkanats { get; set; }
        public DbSet<Sabeghe> Sabeghes { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            builder.Entity<AppUserRole>(entity =>
            {
                entity.Property(e => e.CodeOstan).HasMaxLength(50);
                entity.Property(e => e.CodeMarkaz).HasMaxLength(50);
            });
            builder.Entity<Daneshjoo>().HasIndex(d=>d.ShomareDaneshjooee).IsUnique();
        }
    }
}
