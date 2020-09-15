using EfCoreSamples.Logging.Persistence;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace EfCoreSamples.Logging.Web
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public static bool UseSqlService { get; set; } = false;

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();

            // Added a sample service for testing out logging.
            services.AddTransient<ITwitterService, TwitterService>();

            // By default we are adding SQL Server DB context.
            services.AddDbContextPool<TwitterDbContext>(options =>
            {
                if (UseSqlService)
                {
                    // You can also use SQL Server.
                    options.UseSqlServer(
                        Configuration.GetConnectionString("TwitterSampleDB"),
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

            // However, we can also use SQLite instead or other DB providers!
            //services.AddDbContext<TwitterDbContext>(options =>
            //    options.UseSqlite("Data Source=Twitter.db", x => { })
            //);

            services.AddTransient<DbContextInitializer<TwitterDbContext>>();
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
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
        }
    }
}
