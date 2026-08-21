namespace PlaystationSystem.Repositoriy
{
    public interface ICurrentTenantRepositoriy
    {
        string? TenantId { get; }
        bool IsSuperAdmin { get; }
    }
}
