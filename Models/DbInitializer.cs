using Microsoft.AspNetCore.Identity;

namespace PlaystationSystem.Models
{
    public class DbInitializer
    {
        public static async Task SeedRolesAndAdminAsync(IServiceProvider serviceProvider)
        {
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();
            var userManager = serviceProvider.GetRequiredService<UserManager<ApplicationUser>>();

            // 1. إنشاء الرتب إذا لم تكن موجودة
            string[] roleNames = { "SuperAdmin", "Admin", "Cashier" };
            foreach (var roleName in roleNames)
            {
                if (!await roleManager.RoleExistsAsync(roleName))
                {
                    await roleManager.CreateAsync(new IdentityRole(roleName));
                }
            }

            // 2. فحص وإنشاء حساب SuperAdmin
            var superAdmin = await userManager.FindByNameAsync("superadmin")
                             ?? await userManager.FindByEmailAsync("superadmin@system.com");

            if (superAdmin == null)
            {
                var newAdmin = new ApplicationUser
                {
                    UserName = "superadmin",
                    Email = "superadmin@system.com",
                    FullName = "مدير النظام العام",
                    PhoneNumber = "01000000000",
                    IsActive = true,
                    EmailConfirmed = true
                };

                var result = await userManager.CreateAsync(newAdmin, "Abdo0106@");
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(newAdmin, "SuperAdmin");
                    Console.WriteLine("--> تم إنشاء حساب السوبر أدمن بنجاح!");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        Console.WriteLine($"--> فشل إنشاء السوبر أدمن: {error.Description}");
                    }
                }
            }
        }
    }
}