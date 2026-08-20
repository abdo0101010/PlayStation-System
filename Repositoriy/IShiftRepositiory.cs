using PlaystationSystem.Models;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Repositoriy
{
    public interface IShiftRepositiory
    {
        public Task<List<Shifts>> GetDescShift();
        public Task<Shifts?> GetActiveShiftAsync();
        public Task<CloseShiftViewModel> PrepareCloseShiftSummaryAsync(Shifts activeShift, string cashierName);

    }
}
