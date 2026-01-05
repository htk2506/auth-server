using AuthServer.Database.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthServer.Database
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

        // Models 
        public DbSet<AppUser> AppUsers { get; set; }
        public DbSet<UserSession> UserSessions { get; set; }

        protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
        {
            optionsBuilder.AddInterceptors(new CustomSaveChangesInterceptor());
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Filter out soft-deleted users
            modelBuilder.Entity<AppUser>().HasQueryFilter(appUser => !appUser.IsDeleted);

            // Filter out sessions for soft-deleted users
            modelBuilder.Entity<UserSession>().HasQueryFilter(userSession => !userSession.AppUser.IsDeleted);

            base.OnModelCreating(modelBuilder);
        }
    }
}
