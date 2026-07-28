using PlaystationSystem.Repositoriy;

namespace PlaystationSystem.Services
{
    public interface IGenericService<TEntity>: IGenericRepository<TEntity> where TEntity : class
    {
    }
}
