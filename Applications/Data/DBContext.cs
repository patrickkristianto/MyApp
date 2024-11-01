using Applications.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Applications.Data
{
    public class DBContext : IdentityDbContext<Users>
    {
        public DbSet<License> Licenses { get; set; }
        public DBContext(DbContextOptions options) : base(options)
        {

        }
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<License>(entity =>
            {
                entity.HasKey(e => e.LicenseId);

                entity.Property(e => e.LicenseKey)
                    .IsRequired()
                    .HasMaxLength(100);

                entity.Property(e => e.SubscriptionLevel)
                    .IsRequired()
                    .HasMaxLength(50);

                entity.Property(e => e.ExpirationDate)
                    .IsRequired();

                entity.Property(e => e.IsActive)
                    .IsRequired();
            });
        }
    }
}
