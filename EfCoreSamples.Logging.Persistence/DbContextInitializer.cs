using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace EfCoreSamples.Logging.Persistence;

/// <summary>
/// Generic DbContext initializer specialized in correctly initializing SQL Server, SQLite, SQLite in-memory, Postgress and In-memory DB engines.
/// For ASP.NET Core application, it should run in Program.cs before the application has fully started.
/// For tests and scoped work, you can use this on initialization of the test/scoped work.
/// NOTE: When using `_context.Database.EnsureCreatedAsync()` before migration, it will fail
///       because `EnsureCreatedAsync` creates all of the tables but not migration table!
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

    public async Task Initialize(CancellationToken ct = default)
    {
        _logger.LogTrace("Initializing {DbContext} DB...", typeof(TDbContext).Name);

        if (OpenConnectionRequired(_context))
        {
            // SQLite needs to open connection first.
            await _context.Database.OpenConnectionAsync(ct);
        }

        if (IsMigrationSupported(_context))
        {
            // Add log context for migration queries, so they can more easily filtered in logs.
            using (_logger.QueryScope("Migrations"))
            {
                var migrations = await _context.Database.GetPendingMigrationsAsync(ct);
                var numberOfPendingMigrations = migrations.Count();

                // Removed `numberOfPendingMigrations > 0` because some empty DBs can have 0 pending migrations even though they are empty.
                // I'm not sure why this is a problem only on some DBs while on most pending migrations number is correct.
                _logger.LogInformation("Starting with {NumberOfMigrations} migrations...", numberOfPendingMigrations);

                // Run DB migration in transaction to make sure we don't partially migrate the DB in case of an error.
                // Partial migrations can be a big problem to solve.
                using var transaction = await _context.Database.BeginTransactionAsync(ct);

                await _context.Database.MigrateAsync(ct);

                await transaction.CommitAsync(ct);
            }
        }
        else
        {
            _logger.LogTrace("Creating DB for {DbContext}...", typeof(TDbContext).Name);

            await _context.Database.EnsureCreatedAsync(ct);
        }
    }

    public bool IsMigrationSupported(DbContext context)
        => context.Database.ProviderName is not
            ("Microsoft.EntityFrameworkCore.InMemory" or "Microsoft.EntityFrameworkCore.Sqlite");

    public bool OpenConnectionRequired(DbContext context)
        => context.Database.ProviderName == "Microsoft.EntityFrameworkCore.Sqlite";

    public bool IsContextForUnitTesting(DbContext context)
        => context.Database.ProviderName is
            "Microsoft.EntityFrameworkCore.InMemory" or "Microsoft.EntityFrameworkCore.Sqlite";
}
