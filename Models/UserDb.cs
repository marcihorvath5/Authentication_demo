using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace authDemo.Models
{
    public class UserDb: IdentityDbContext<User>
    {
        public UserDb(DbContextOptions<UserDb> options): base(options)
        {
            
        }

        public DbSet<User> MyUsers { get; set; }
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        public DbSet<FileModel> Files { get; set; }

        protected override void OnModelCreating(ModelBuilder builder)
        {
           base.OnModelCreating(builder);
        }
    }
}
