using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using PlaystationSystem.Models;
using System.Security.Claims;

namespace PlaystationSystem.Services
{
    public class ApplicationUserClaimsPrincipalFactory: UserClaimsPrincipalFactory<ApplicationUser, IdentityRole>
    {
        public ApplicationUserClaimsPrincipalFactory(
              UserManager<ApplicationUser> userManager,
              RoleManager<IdentityRole> roleManager,
              IOptions<IdentityOptions> options)
              : base(userManager, roleManager, options)
        {
        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(ApplicationUser user)
        {
            // 1. توليد الـ Claims الافتراضية
            var identity = await base.GenerateClaimsAsync(user);

            // 2. إضافة الـ FullName للـ Cookie
            if (!string.IsNullOrEmpty(user.FullName))
            {
                identity.AddClaim(new Claim("FullName", user.FullName));
            }

            return identity;
        }
    }
}
