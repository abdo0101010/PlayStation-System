using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;

namespace PlaystationSystem.Repositoriy
{
    public class ShiftRepositiory:IShiftRepositiory
    {
        private readonly ApplicationDbContext _context;

        public ShiftRepositiory(ApplicationDbContext context)
        {
            _context = context;
            
        }
        public async Task<List< Shifts>> GetDescShift()
        {
            var shifts = await _context.Shifts
        .Include(s => s.User)
        .OrderByDescending(s => s.StartTime)
        .ToListAsync();
            return shifts;

        }
        public async Task<Shifts?> GetActiveShiftAsync()
        {
            // يرمي Exception لو تم العثور على أكثر من وردية مفتوحة في قاعدة البيانات بسبب خطأ ما
            return await _context.Shifts
                .Include(s => s.User)
                .SingleOrDefaultAsync(s => s.IsOpen);
        }

    }
}
