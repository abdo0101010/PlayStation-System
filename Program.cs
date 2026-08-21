using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.Repositoriy;
using PlaystationSystem.Services;
using System.Threading.Tasks;
using static System.Formats.Asn1.AsnWriter;

namespace PlaystationSystem
{
    public class Program
    {
        public static async Task Main(string[] args)
        {

            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));

            builder.Services.AddScoped<IAdminRepositoriy, AdminRepository>();
            builder.Services.AddScoped<IAdminServices, AdminServices>();
            builder.Services.AddScoped(typeof(IGenericRepository<>), typeof(GenericRepository<>));
            builder.Services.AddScoped(typeof(IGenericService<>), typeof(GenericService<>));
            builder.Services.AddScoped<IShiftRepositiory, ShiftRepositiory>();
            builder.Services.AddScoped<IShiftServices, ShiftServices>();
            builder.Services.AddScoped<ICurrentTenantRepositoriy, CurrentTenantRepositoriy>();
            builder.Services.AddScoped<ICurrentTenantService, CurrentTenantService>();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>();  

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/Account/Login"; // Set the login path
                options.AccessDeniedPath = "/Account/AccessDenied"; // Set the access denied path
            });
            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();
            var app = builder.Build();
            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication(); // 👈 1. يقرأ التوثيق ويكريت الـ Cookie
            app.UseAuthorization();
            app.UseMiddleware<SubscriptionValidationMiddleware>();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            using(var scope = app.Services.CreateScope())
{
                var services = scope.ServiceProvider;
                try
                {
                    await DbInitializer.SeedRolesAndAdminAsync(services);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error seeding data: {ex.Message}");
                }
            }
            app.Run();
        }
    }
}
