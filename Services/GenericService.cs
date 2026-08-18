using PlaystationSystem.Repositoriy;
using System.Linq.Expressions;

namespace PlaystationSystem.Services
{
    public class GenericService<TEntity> : IGenericService<TEntity> where TEntity : class
    {
        private readonly IGenericRepository<TEntity> _repository;

        public GenericService(IGenericRepository<TEntity> repository)
        {
            _repository = repository;
        }

        public async Task<TEntity?> GetByIdAsync(string id)
        {
            return await _repository.GetByIdAsync(id);
        }
        public async Task<IEnumerable<TEntity>> GetAllAsync()
        {
            return await _repository.GetAllAsync();
        }
        public async Task<IEnumerable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return await _repository.FindAsync(predicate);
        }
        public async Task AddAsync(TEntity entity)
        {
            await _repository.AddAsync(entity);
        }
        public async Task Update(TEntity entity)
        {
            await _repository.Update(entity);
        }
        public async Task Delete(TEntity entity)
        {
            await _repository.Delete(entity);
        }
      

        public Task<int> SaveChangesAsync()
        {
            return _repository.SaveChangesAsync();

        }
        public async Task DeleteById(string id)
        {
            await _repository.DeleteById(id);
        }
        // Implement methods from IGenericService<TEntity>
    }
}
