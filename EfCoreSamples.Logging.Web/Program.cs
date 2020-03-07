using System;
using System.Diagnostics;
using System.Threading.Tasks;
using EfCoreSamples.Logging.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Serilog;

namespace EfCoreSamples.Logging.Web
{
    /// <summary>
    /// Logging based on https://jkdev.me/asp-net-core-serilog/
    /// </summary>
    public class Program
    {
        public async static Task Main(string[] args)
        {
            try
            {
                using IHost host = CreateHostBuilder(args).Build();
                if (!await ApplyDbMigrations(host))
                {
                    return;
                }

                host.Run();
            }
            catch (Exception ex)
            {
                // Log.Logger will likely be internal type "Serilog.Core.Pipeline.SilentLogger".
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
                Log.CloseAndFlush();
            }
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseStartup<Startup>()
                     .CaptureStartupErrors(true)
                     .ConfigureAppConfiguration(config =>
                     {
                         config
                             // Used for local settings like connection strings.
                             .AddJsonFile("appsettings.Local.json", optional: true);
                     })
                     .UseSerilog((hostingContext, loggerConfiguration) => {
                         loggerConfiguration
                             .ReadFrom.Configuration(hostingContext.Configuration)
                             .Enrich.FromLogContext()
                             .Enrich.WithProperty("ApplicationName", typeof(Program).Assembly.GetName().Name)
                             .Enrich.WithProperty("Environment", hostingContext.HostingEnvironment);

#if DEBUG
                            // Used to filter out potentially bad data due debugging.
                            // Very useful when doing Seq dashboards and want to remove logs under debugging session.
                            loggerConfiguration.Enrich.WithProperty("DebuggerAttached", Debugger.IsAttached);
#endif
                        });
                });

        private static async Task<bool> ApplyDbMigrations(IHost host)
        {
            using (IServiceScope scope = host.Services.CreateScope())
            {
                IServiceProvider services = scope.ServiceProvider;
                try
                {
                    var dbInitializer = services.GetRequiredService<DbContextInitializer<TwitterDbContext>>();
                    await dbInitializer.Initialize().ConfigureAwait(false);
                    return true;
                }
                catch (Exception ex)
                {
                    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
                    logger.LogCritical(ex, "An error occurred while migrating or initializing the database.");
                    return false;
                }
            }
        }
    }
}
