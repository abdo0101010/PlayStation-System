using Microsoft.AspNetCore.Identity;
using PlaystationSystem.Models;
using System.Security.Claims;

namespace PlaystationSystem.Repositoriy
{
    public class CurrentTenantRepositoriy: ICurrentTenantRepositoriy
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IServiceProvider _serviceProvider;
        public CurrentTenantRepositoriy(IHttpContextAccessor httpContextAccessor, IServiceProvider serviceProvider)
        {
            _httpContextAccessor = httpContextAccessor;
            _serviceProvider = serviceProvider;
        }

        public string? TenantId
        {
            get
            {
                var userPrincipal = _httpContextAccessor.HttpContext?.User;
                if (userPrincipal?.Identity?.IsAuthenticated != true)
                    return null;

                // 1. محاولة القراءة من الـ Claim أولاً
                var tenantClaim = userPrincipal.FindFirst("TenantId")?.Value;
                if (!string.IsNullOrEmpty(tenantClaim))
                    return tenantClaim;

                // 2. إذا لم يكن موجوداً في الـ Claim، يتم جلبه مباشرة من الـ UserManager
                using var scope = _serviceProvider.CreateScope();
                var userManager = scope.ServiceProvider.GetRequiredService<UserManager<ApplicationUser>>();
                var userId = userPrincipal.FindFirstValue(ClaimTypes.NameIdentifier);

                if (!string.IsNullOrEmpty(userId))
                {
                    var user = userManager.FindByIdAsync(userId).GetAwaiter().GetResult();
                    return user?.TenantId;
                }

                return null;
            }
        }
        public bool IsSuperAdmin => _httpContextAccessor.HttpContext?.User?.IsInRole("SuperAdmin") ?? false;
    }
}
