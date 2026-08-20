using PlaystationSystem.Models;
using PlaystationSystem.Repositoriy;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Services
{
    public class ShiftServices:IShiftServices
    {
        private readonly IShiftRepositiory shiftRepositiory;

        public ShiftServices(IShiftRepositiory _shiftRepositiory)
        {
            shiftRepositiory = _shiftRepositiory;
            
        }

        public async Task<List<Shifts>> GetDescShift()
        {
            var shift=await shiftRepositiory.GetDescShift();
            if (shift != null) 
                return shift;

            return null;
        }
        public async Task<Shifts?> GetActiveShiftAsync()
        {
           var active=await shiftRepositiory.GetActiveShiftAsync();
            return active;
        }
         public async Task<CloseShiftViewModel> PrepareCloseShiftSummaryAsync(Shifts activeShift, string cashierName)
        {
            var summary = await shiftRepositiory.PrepareCloseShiftSummaryAsync(activeShift, cashierName);
            return summary;

        }

    }
}
