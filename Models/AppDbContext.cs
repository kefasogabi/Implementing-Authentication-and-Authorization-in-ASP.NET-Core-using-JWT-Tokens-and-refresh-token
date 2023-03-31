using Microsoft.EntityFrameworkCore;

namespace JwtAuth.Models
{
    public class AppDbContext : DbContext
    {
        public AppDbContext()
        {    
        }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        public virtual DbSet<UserProfile> UserProfiles { get; set; }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

        }
    }
}
