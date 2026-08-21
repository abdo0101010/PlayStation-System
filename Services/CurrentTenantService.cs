using PlaystationSystem.Repositoriy;

namespace PlaystationSystem.Services
{
    public class CurrentTenantService : ICurrentTenantService
    {
        private readonly ICurrentTenantRepositoriy currentTenantRepositoriy;
        public CurrentTenantService(ICurrentTenantRepositoriy currentTenantRepositoriy)
        {
            this.currentTenantRepositoriy = currentTenantRepositoriy;
        }

        public string? TenantId => currentTenantRepositoriy.TenantId;

        public bool IsSuperAdmin => currentTenantRepositoriy.IsSuperAdmin;
    }
}
