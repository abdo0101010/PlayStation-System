using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using System.Linq.Expressions;

namespace PlaystationSystem.Repositoriy
{
    public class GenericRepository<T> : IGenericRepository<T> where T : class
    {
        private readonly ApplicationDbContext _context;
        private readonly DbSet<T> entities;
        private readonly ICurrentTenantRepositoriy _currentTenantRepository;

        public GenericRepository(ApplicationDbContext context, ICurrentTenantRepositoriy currentTenantRepository)
        {
            _context = context;
            entities = context.Set<T>();
            _currentTenantRepository = currentTenantRepository;
        }

        public async Task AddAsync(T entity)
        {
            // إسناد الـ TenantId تلقائياً قبل الحفظ إذا كانت الخاصية موجودة وفارغة
            var tenantProp = typeof(T).GetProperty("TenantId");
            if (tenantProp != null && tenantProp.CanWrite)
            {
                var currentValue = tenantProp.GetValue(entity) as string;
                if (string.IsNullOrEmpty(currentValue))
                {
                    tenantProp.SetValue(entity, _currentTenantRepository.TenantId);
                }
            }

            await _context.AddAsync(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Update(T entity)
        {
            if (entity == null) return;

            _context.Update(entity);
            await _context.SaveChangesAsync();
        }

        public async Task Delete(int id)
        {
            if (id <= 0) return;

            var entity = await entities.FindAsync(id);
            if (entity != null)
            {
                _context.Remove(entity);
                await _context.SaveChangesAsync();
            }
        }

        // جلب عنصر بالـ ID مع التأكد من ملكيته للفرع الحالي
        public async Task<T?> GetByIdAsync(string id)
        {
            IQueryable<T> query = entities;

            if (typeof(T).GetProperty("TenantId") != null && !_currentTenantRepository.IsSuperAdmin)
            {
                var currentTenantId = _currentTenantRepository.TenantId;
                query = query.Where(e => EF.Property<string>(e, "TenantId") == currentTenantId);
            }

            return await query.FirstOrDefaultAsync(e => EF.Property<string>(e, "Id") == id);
        }

        // جلب كل العناصر مع تطبيق عزل الـ Multi-Tenancy
        public async Task<IEnumerable<T>> GetAllAsync()
        {
            IQueryable<T> query = entities;

            if (typeof(T).GetProperty("TenantId") != null && !_currentTenantRepository.IsSuperAdmin)
            {
                var currentTenantId = _currentTenantRepository.TenantId;
                query = query.Where(e => EF.Property<string>(e, "TenantId") == currentTenantId);
            }

            return await query.ToListAsync();
        }

        public async Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate)
        {
            IQueryable<T> query = entities;

            if (typeof(T).GetProperty("TenantId") != null && !_currentTenantRepository.IsSuperAdmin)
            {
                var currentTenantId = _currentTenantRepository.TenantId;
                query = query.Where(e => EF.Property<string>(e, "TenantId") == currentTenantId);
            }

            return await query.Where(predicate).ToListAsync();
        }

        public async Task<int> SaveChangesAsync()
        {
            return await _context.SaveChangesAsync();
        }

        public async Task Delete(T entity)
        {
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            entities.Remove(entity);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteById(string id)
        {
            var entity = await GetByIdAsync(id);
            if (entity == null)
            {
                throw new ArgumentNullException(nameof(entity));
            }
            entities.Remove(entity);
            await _context.SaveChangesAsync();
        }
    }
}