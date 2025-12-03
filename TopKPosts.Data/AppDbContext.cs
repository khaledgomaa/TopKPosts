using Microsoft.EntityFrameworkCore;

namespace TopKPosts.Data
{
    public class AppDbContext : DbContext
    {
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        public DbSet<Post> Posts { get; set; }
        public DbSet<Like> Likes { get; set; }
    }
}
