using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using System.Linq.Expressions;

namespace PlaystationSystem.Repositoriy
{
    public class GenericRepository<T>: IGenericRepository<T> where T : class
    {
        private ApplicationDbContext _context;
        private DbSet<T> entities;
        public GenericRepository(ApplicationDbContext context) {
            _context = context;
            entities = context.Set<T>();
        }
        public async Task AddAsync(T entity) {
           await  _context.AddAsync( entity );
        }
        public async Task Update(T Entity)
        {
            if (entities == null)
            {
                return;
            }
            _context.Update(Entity);
        }
        public async Task Delete(int id)
        {
            var entity = await entities.FindAsync(id);
            if (id <= 0)
            {
                return;
            }
             _context.Remove(entity);
        }
        public async Task<T?> GetByIdAsync(int id)
        {
            return await entities.FindAsync(id);
        }

        // 5. جلب كل العناصر
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            return await entities.ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }
        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            return await entities.Where(predicate).ToListAsync();
        }


       

        public async Task Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
         entities.Remove(entity);
        }  
        

        
    }
}
