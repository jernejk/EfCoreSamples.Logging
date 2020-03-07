using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace EfCoreSamples.Logging.Persistence
{
    /// <summary>
    /// Generic DbContext initializers specialized in 2 things:
    /// 1. Ensure the DB is correctly migrated when using SQL Server
    /// 2. Ensure the DB is correctly initialized for in-memory/SQLite mostly used in (integration) testing
    /// </summary>
    /// <typeparam name="TDbContext">The DbContext type that needs to be initialized</typeparam>
    public class DbContextInitializer<TDbContext>
        where TDbContext : DbContext
    {
        private readonly TDbContext _context;
        private readonly ILogger _logger;

        public DbContextInitializer(TDbContext context, ILogger<DbContextInitializer<TDbContext>> logger)
        {
            _context = context;
            _logger = logger;
        }

        public async Task Initialize()
        {
            _logger.LogTrace("Initializing {DbContext} DB...", typeof(TDbContext).Name);

            if (OpenConnectionRequired(_context))
            {
                // SQLite needs to open connection first.
                await _context.Database.OpenConnectionAsync();
            }

            if (IsMigrationSupported(_context))
            {
                var migrations = await _context.Database.GetPendingMigrationsAsync();
                int numberOfPendingMigrations = migrations.Count();
                if (numberOfPendingMigrations > 0)
                {
                    _logger.LogInformation("Starting with {NumberOfMigrations} migrations...", numberOfPendingMigrations);

                    // Add log context for migration queries, so they can more easily filtered in logs.
                    using (_logger.EFQueryScope("Migrations"))
                    {
                        // Not supported by in-memory DBs and SQLite.
                        await _context.Database.MigrateAsync();
                    }
                }
            }
            else
            {
                _logger.LogTrace("Creating DB for {DbContext}...", typeof(TDbContext).Name);

                await _context.Database.EnsureCreatedAsync();
            }
        }

        public static bool IsMigrationSupported(DbContext context)
            => context.Database.ProviderName != "Microsoft.EntityFrameworkCore.InMemory"
            && context.Database.ProviderName != "Microsoft.EntityFrameworkCore.Sqlite";

        public static bool OpenConnectionRequired(DbContext context)
            => context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

        public static bool IsContextForUnitTesting(DbContext context)
            => context.Database.ProviderName == "Microsoft.EntityFrameworkCore.InMemory"
            || context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";
    }
}
