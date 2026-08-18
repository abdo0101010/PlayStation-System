using PlaystationSystem.Models;
using PlaystationSystem.Repositoriy;

namespace PlaystationSystem.Services
{
    public class AdminServices : IAdminServices
    {
        private readonly IAdminRepositoriy _adminRepo;

        public AdminServices(IAdminRepositoriy adminRepo)
        {
            _adminRepo = adminRepo;
        }

     
    

        public async Task<List<Shifts>> GetAllShiftsAsync()
        {
            return await _adminRepo.GetAllShiftsAsync();
        }

        public async Task<Shifts?> GetShiftByIdAsync(int id)
        {
            return await _adminRepo.GetShiftByIdAsync(id);
        }

        public async Task<Shifts> AddShiftAsync(Shifts shift)
        {
            return await _adminRepo.AddShiftAsync(shift);
        }

        public async Task<List<Session>> GetAllSessionsAsync()
        {
            return await _adminRepo.GetAllSessionsAsync();
        }

        public async Task<Session?> GetSessionByIdAsync(int id)
        {
            return await _adminRepo.GetSessionByIdAsync(id);
        }

        public async Task<Session?> GetSessionByIdAsync(string id, bool includeOrders)
        {
            return await _adminRepo.GetSessionByIdAsync(id, includeOrders);
        }
    }
}