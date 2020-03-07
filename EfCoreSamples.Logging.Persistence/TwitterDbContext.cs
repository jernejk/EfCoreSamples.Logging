using EfCoreSamples.Logging.Persistence.Entities;
using Microsoft.EntityFrameworkCore;

namespace EfCoreSamples.Logging.Persistence
{
    /// <summary>
    /// DB Context for Twitter sample DB.
    /// 
    /// Add migrations in cmd:
    /// EFCoreSamples.Logging.Web> dotnet ef migrations add Init -p ..\EFCoreSamples.Logging.Persistence -v
    /// 
    /// Remove migrations:
    /// EFCoreSamples.Logging.Web> dotnet ef migrations remove -p ..\EFCoreSamples.Logging.Persistence -v
    /// </summary>
    public class TwitterDbContext : DbContext
    {
        public DbSet<Tweet> Tweets { get; set; }

        public TwitterDbContext(DbContextOptions<TwitterDbContext> options)
            : base(options)
        {
        }
    }
}
