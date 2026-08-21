using Microsoft.AspNetCore.Identity;

namespace PlaystationSystem.Models
{
    public class SubscriptionValidationMiddleware
    {
        private readonly RequestDelegate _next;

        public SubscriptionValidationMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, UserManager<ApplicationUser> userManager, SignInManager<ApplicationUser> signInManager)
        {
            if (context.User.Identity?.IsAuthenticated == true && !context.User.IsInRole("SuperAdmin"))
            {
                var user = await userManager.GetUserAsync(context.User);

                if (user != null)
                {
                    // إذا تم تعطيل الحساب يدوياً أو انتهى تاريخ الصلاحية
                    bool isExpired = user.SubscriptionEndDate.HasValue && user.SubscriptionEndDate.Value < DateTime.UtcNow;

                    if (!user.IsActive || isExpired)
                    {
                        await signInManager.SignOutAsync();
                        context.Response.Redirect("/Account/Login?error=expired");
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
