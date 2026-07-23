using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;

namespace PlaystationSystem.Repositoriy
{
    public class AdminRepositoriy : IAdminRepositoriy
    {
        private readonly ApplicationDbContext _context;

        public AdminRepositoriy(ApplicationDbContext context)
        {
            _context = context;
        }

        #region User Operations
        public async Task<List<User>> GetAllUsersAsync()
        {
            return await _context.Users.ToListAsync();
        }

        public async Task<User?> GetUserByIdAsync(int id)
        {
            return await _context.Users.FindAsync(id);
        }

        // تعديل نوع الإرجاع لـ Task<User> عشان يتوافق مع الـ Interface
        public async Task<User> AddUserAsync(User user)
        {
            await _context.Users.AddAsync(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(User user)
        {
            var existingUser = await _context.Users.FindAsync(user.Id);
            if (existingUser == null)
            {
                return null;
            }

            existingUser.Username = user.Username;
            existingUser.PhoneNumber = user.PhoneNumber;
            existingUser.Password = user.Password;
            existingUser.Role = user.Role;
            existingUser.IsActive = user.IsActive;

            await _context.SaveChangesAsync();
            return existingUser;
        }

        public async Task DeleteUserAsync(int id)
        {
            var user = await _context.Users.FindAsync(id);
            if (user != null)
            {
                _context.Users.Remove(user);
                await _context.SaveChangesAsync();
            }
        }
        #endregion

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

        public async Task<Session?> GetSessionByIdAsync(int id, bool includeOrders)
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