using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;

namespace PlaystationSystem.Repositoriy
{
    public class AdminRepository : IAdminRepositoriy
    {
        private readonly ApplicationDbContext _context;

        public AdminRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        #region Shift Operations
        public async Task<List<Shifts>> GetAllShiftsAsync()
        {
            return await _context.Shifts.ToListAsync();
        }

        public async Task<Shifts?> GetShiftByIdAsync(int id)
        {
            return await _context.Shifts.FindAsync(id);
        }

        public async Task<Shifts> AddShiftAsync(Shifts shift)
        {
            await _context.Shifts.AddAsync(shift);
            await _context.SaveChangesAsync();
            return shift;
        }
        #endregion

        #region Session Operations
        public async Task<List<Session>> GetAllSessionsAsync()
        {
            return await _context.Sessions.ToListAsync();
        }

        public async Task<Session?> GetSessionByIdAsync(int id)
        {
            return await _context.Sessions.FindAsync(id);
        }

        public async Task<Session?> GetSessionByIdAsync(string id, bool includeOrders)
        {
            if (includeOrders)
            {
                return await _context.Sessions
                    .Include(s => s.Orders)
                    .FirstOrDefaultAsync(s => s.Id == id);
            }

            return await _context.Sessions.FindAsync(id);
        }
        #endregion
    }
}