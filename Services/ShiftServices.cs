using PlaystationSystem.Models;
using PlaystationSystem.Repositoriy;

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

    }
}
