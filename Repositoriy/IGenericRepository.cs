using System.Linq.Expressions;

namespace PlaystationSystem.Repositoriy
{
    public interface IGenericRepository<T> where T : class
    {
        Task<T?> GetByIdAsync(int id);
        Task<IEnumerable<T>> GetAllAsync();
        Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate);
        Task AddAsync(T entity);
        public Task Update(T Entity);
        public Task Delete(T entity);
        Task<int> SaveChangesAsync();
        public  Task DeleteById(int id);

    }
}
