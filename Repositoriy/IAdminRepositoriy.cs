using PlaystationSystem.Models;

namespace PlaystationSystem.Repositoriy
{
    public interface IAdminRepositoriy
    {
        public Task<List<User>> GetAllUsersAsync();
        public Task<User?> GetUserByIdAsync(int id);
        public Task<User> AddUserAsync(User user);
        public Task<User?> UpdateUserAsync(User user);
        public Task DeleteUserAsync(int id);
        public Task<List<Shifts>> GetAllShiftsAsync();
        public Task<Shifts?> GetShiftByIdAsync(int id);
        public Task<Shifts> AddShiftAsync(Shifts shift);
        public Task<Session?> GetSessionByIdAsync(int id);
        public Task<List<Session>> GetAllSessionsAsync();
        public Task<Session?> GetSessionByIdAsync(int id, bool includeOrders);


    }
}
