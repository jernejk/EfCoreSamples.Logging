// Logging based on https://jkdev.me/asp-net-core-serilog/ and https://github.com/datalust/dotnet6-serilog-example
// NOTE: When upgrading from .NET 5 or earlier, add `<ImplicitUsings>enable</ImplicitUsings>` to **.csproj** file under `<PropertyGroup>`.
// NOTE: While you can still use full Program.cs and Startup.cs, `.UseSerilog()` is marked as obsolete for them. It's safer to move to minimal APIs.
using EfCoreSamples.Logging.Persistence;
using Microsoft.EntityFrameworkCore;
using Serilog;
using System.Diagnostics;

try
{
    var builder = WebApplication.CreateBuilder(args);

    builder.Host.UseSerilog((ctx, loggerConfiguration) =>
    {
        loggerConfiguration
            .ReadFrom.Configuration(ctx.Configuration)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
            .Enrich.WithProperty("Environment", ctx.HostingEnvironment);

#if DEBUG
        // Used to filter out potentially bad data due debugging.
        // Very useful when doing Seq dashboards and want to remove logs under debugging session.
        loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
    });

    // Register required services.
    RegisterServices(builder.Services, builder.Configuration, useSqlServer: true);

    WebApplication app = builder.Build();
    if (!await ApplyDbMigrations(app))
    {
        // TODO: Decide what to do with the application if DB migrations failed.
        return;
    }

    // Required for logging requests!
    app.UseSerilogRequestLogging();

    // Rest of configuration.
    if (app.Environment.IsDevelopment())
    {
        app.UseDeveloperExceptionPage();
    }
    else
    {
        app.UseExceptionHandler("/Home/Error");
        // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
        app.UseHsts();
    }
    app.UseHttpsRedirection();
    app.UseStaticFiles();

    app.UseRouting();
    app.UseAuthorization();

    app.UseEndpoints(endpoints =>
    {
        endpoints.MapControllerRoute(
            name: "default",
            pattern: "{controller=Home}/{action=Index}/{id?}");
    });

    app.Run();
}
catch (Exception ex)
{
    if (Log.Logger == null || Log.Logger.GetType().Name == "SilentLogger")
    {
        // Loading configuration or Serilog failed.
        // This will create a logger that can be captured by Azure logger.
        // To enable Azure logger, in Azure Portal:
        // 1. Go to WebApp
        // 2. App Service logs
        // 3. Enable "Application Logging (Filesystem)", "Application Logging (Filesystem)" and "Detailed error messages"
        // 4. Set Retention Period (Days) to 10 or similar value
        // 5. Save settings
        // 6. Under Overview, restart web app
        // 7. Go to Log Stream and observe the logs
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Console()
            .CreateLogger();
    }

    Log.Fatal(ex, "Host terminated unexpectedly");
}
finally
{
    Log.Information("Shut down complete");
    Log.CloseAndFlush();
}

static async Task<bool> ApplyDbMigrations(IHost host)
{
    // Service scope will ensure the DbContext is disposed after migration is done.
    using IServiceScope scope = host.Services.CreateScope();

    IServiceProvider services = scope.ServiceProvider;
    try
    {
        var dbInitializer = services.GetRequiredService<DbContextInitializer<TwitterDbContext>>();
        await dbInitializer.Initialize().ConfigureAwait(false);
        return true;
    }
    catch (Exception ex)
    {
        Log.Fatal(ex, "An error occurred while migrating or initializing the database.");
        return false;
    }
}

static void RegisterServices(IServiceCollection services, IConfiguration configuration, bool useSqlServer)
{
    services.AddControllersWithViews();
    services.AddAuthorization();

    // Added a sample service for testing out logging.
    services.AddTransient<ITwitterService, TwitterService>();

    // By default we are adding SQL Server DB context.
    services.AddDbContextPool<TwitterDbContext>(options =>
    {
        if (useSqlServer)
        {
            // You can also use SQL Server.
            options.UseSqlServer(
                configuration.GetConnectionString("TwitterSampleDB"),
                b => b.MigrationsAssembly("EfCoreSamples.Logging.Persistence"));
        }
        else
        {
            // Using SQLite by default as it is easier setup.
            options.UseSqlite("Data Source=db.sqlite");
        }


#if DEBUG
        // Most project shouldn't expose sensitive data, which is why we are
        // limiting to be available only in DEBUG mode.
        // If this is not, SQL "parameters" will be '?' instead of actual values.
        options.EnableSensitiveDataLogging();
#endif
    });

    // Used for DB migrations.
    services.AddTransient<DbContextInitializer<TwitterDbContext>>();
}