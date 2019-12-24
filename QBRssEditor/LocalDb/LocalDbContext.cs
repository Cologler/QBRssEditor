using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using QBRssEditor.Abstractions;

namespace QBRssEditor.LocalDb
{
    class LocalDbContext : DbContext
    {
        public LocalDbContext(DbContextOptions options) : base(options) { }

        public DbSet<ResourceItem> Resources { get; set; }
    }
}
