using PlaystationSystem.Models;

namespace PlaystationSystem.Repositoriy
{
    public interface IShiftRepositiory
    {
        public Task<List<Shifts>> GetDescShift();
        public Task<Shifts?> GetActiveShiftAsync();

    }
}
