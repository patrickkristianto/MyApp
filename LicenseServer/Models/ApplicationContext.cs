using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace LicenseServer.Models
{
    public partial class ApplicationContext : IdentityDbContext<AspNetUser>
    {
        public ApplicationContext(DbContextOptions<ApplicationContext> options)
            : base(options)
        {
        }

        public virtual DbSet<Licenses> Licenses { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<AspNetUser>(entity =>
            {
                entity.HasIndex(e => e.NormalizedEmail, "EmailIndex");
                entity.HasIndex(e => e.NormalizedUserName, "UserNameIndex")
                    .IsUnique()
                    .HasFilter("([NormalizedUserName] IS NOT NULL)");

                entity.Property(e => e.Email).HasMaxLength(256);
                entity.Property(e => e.NormalizedEmail).HasMaxLength(256);
                entity.Property(e => e.NormalizedUserName).HasMaxLength(256);
                entity.Property(e => e.UserName).HasMaxLength(256);
            });

            modelBuilder.Entity<Licenses>(entity =>
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

                entity.HasOne(e => e.User)
                    .WithMany()
                    .HasForeignKey(e => e.UserId)
                    .OnDelete(DeleteBehavior.SetNull);
            });
        }
    }
}
