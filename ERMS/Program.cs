using ERMS.Data;
using ERMS.Services;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NLog;
using NLog.Web;


namespace ERMS
{
    public class Program
    {
        public static async Task Main(string[] args)
        {
            var logger = NLog.LogManager.Setup().LoadConfigurationFromAppSettings().GetCurrentClassLogger();

            try
            {
                logger.Debug("init main");

                var builder = WebApplication.CreateBuilder(args);

                // Initiate NLog
                builder.Logging.ClearProviders();
                builder.Logging.SetMinimumLevel(Microsoft.Extensions.Logging.LogLevel.Trace);
                builder.Host.UseNLog();

                // Add services to the container.
                var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") ?? throw new InvalidOperationException("Connection string 'DefaultConnection' not found.");
                builder.Services.AddDbContext<ApplicationDbContext>(options =>
                    options.UseSqlServer(connectionString));
                builder.Services.AddDatabaseDeveloperPageExceptionFilter();

                //builder.Services.AddDefaultIdentity<IdentityUser>(options => options.SignIn.RequireConfirmedAccount = true)
                //    .AddEntityFrameworkStores<ApplicationDbContext>();
                builder.Services.AddControllersWithViews();

                builder.Services.AddControllers().
                    AddJsonOptions(options =>
                    {
                        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
                    });

                // This makes sure that the Services are registered.
                // This provides the base address for the HttpClient used in the services.
                // THE BASE URL RIGHT NOW IS NOT THE SAME THAT THE ONE USED WHEN DEPLOYING THE APP
                // THIS BASE url NEEDS TO CHANGE TO 8081 PORT BEFORE DEPLOYING
                builder.Services.AddHttpClient<EmployeeApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://ermsappcapstone.azurewebsites.net/");
                });
                builder.Services.AddHttpClient<ProjectApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://ermsappcapstone.azurewebsites.net/");
                });

                builder.Services.AddHttpClient<TaskItemApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://ermsappcapstone.azurewebsites.net/");
                });

                // Register the services for dependency injection
                builder.Services.AddHttpClient<IEmployeeApiService, EmployeeApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://ermsappcapstone.azurewebsites.net/");
                });
                builder.Services.AddHttpClient<IProjectApiService, ProjectApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://ermsappcapstone.azurewebsites.net/");
                });

                builder.Services.AddHttpClient<ITaskItemApiService, TaskItemApiService>(client =>
                {
                    client.BaseAddress = new Uri("https://ermsappcapstone.azurewebsites.net/");
                });


                // Add Identity services using IdentityUser and ApplicationDBContext
                builder.Services.AddIdentity<IdentityUser, IdentityRole>()
                    .AddEntityFrameworkStores<ApplicationDbContext>()
                    .AddDefaultTokenProviders();

                builder.Services.AddRazorPages();

                // Creates Roles
                async Task SeedRoles(IServiceProvider serviceProvider)
                {
                    var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
                    string[] roleNames = { "Admin", "Manager", "Employee" };
                    IdentityResult roleResult;
                    foreach (var roleName in roleNames)
                    {
                        var roleExist = await roleManager.RoleExistsAsync(roleName);
                        if (!roleExist)
                        {
                            roleResult = await roleManager.CreateAsync(new IdentityRole(roleName));
                        }
                    }
                }

                // Redirect to AccessDenied page if user is not authorized
                builder.Services.ConfigureApplicationCookie(options =>
                {
                    options.AccessDeniedPath = "/Home/AccessDenied"; // or another controller if you prefer
                });


                var app = builder.Build();

                // Call the SeedRoles method to create roles
                using (var scope = app.Services.CreateScope())
                {
                    var services = scope.ServiceProvider;
                    await SeedRoles(services);
                }

                // Configure the HTTP request pipeline.
                if (app.Environment.IsDevelopment())
                {
                    app.UseMigrationsEndPoint();
                }
                else
                {
                    app.UseExceptionHandler("/Home/Error");
                }
                app.UseStaticFiles();

                app.UseRouting();

                // Enable authentication and authorization
                app.UseAuthentication();
                app.UseAuthorization();

                app.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");

                app.MapControllers();
                app.MapRazorPages();

                app.Run();
            }
            catch (Exception ex)
            {
                logger.Error(ex, "Stopped program because of exception");
                throw;
            }
            finally
            {
                NLog.LogManager.Shutdown(); // Ensure to flush and close down internal threads and timers
            }
        }
    }
}
