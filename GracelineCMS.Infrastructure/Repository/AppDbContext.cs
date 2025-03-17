using GracelineCMS.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace GracelineCMS.Infrastructure.Repository
{
    public class AppDbContext : DbContext
    {
        public DbSet<Domain.Entities.Organization> Organizations { get; set; }
        public DbSet<AuthCode> AuthCodes { get; set; }
        public DbSet<User> Users { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<ContentModule> ContentModules { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);
            modelBuilder.Entity<User>()
                .HasIndex(u => u.EmailAddress)
                .IsUnique();
        }
    }
}
