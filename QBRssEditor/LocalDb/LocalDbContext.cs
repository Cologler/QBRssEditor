using Microsoft.EntityFrameworkCore;

namespace QBRssEditor.LocalDb
{
    class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ResourceItem> Resources { get; set; }
    }
}
