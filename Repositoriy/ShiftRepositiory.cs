using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.ViewModel;

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
        public async Task<CloseShiftViewModel> PrepareCloseShiftSummaryAsync(Shifts activeShift, string cashierName)
        {
            var startTime = activeShift.StartTime;

            // 1. حساب إيراد أجهزة البلايستيشن (الجلسات المنتهية خلال فترة الوردية)
            decimal gamingIncome = await _context.Sessions
                .Where(s => !s.IsOpen && s.EndTime >= startTime)
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            // 2. حساب مبيعات البوفيه (الطلبات التابعة للجلسات المنتهية خلال الوردية)
            decimal buffetIncome = await _context.SessionOrders
                .Where(o => !o.Session.IsOpen && o.Session.EndTime >= startTime)
                .SumAsync(o => (decimal?)(o.Quantity * o.UnitPrice)) ?? 0;


            decimal debtCollected = await _context.DebtPayments
        .Where(p => p.PaymentDate >= startTime || p.ShiftId == activeShift.Id)
        .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // 4. حساب المصروفات النثرية
            decimal totalExpenses = await _context.Expenses
                .Where(e => e.CreatedAt >= startTime || e.ShiftId == activeShift.Id)
                .SumAsync(e => (decimal?)e.Amount) ?? 0;

            // حساب الإجمالي المفترض في الدرج
            decimal expectedCash = (activeShift.StartingCash + gamingIncome + buffetIncome + debtCollected) - totalExpenses;

            return new CloseShiftViewModel
            {
                ShiftId = activeShift.Id,
                CashierName = cashierName,
                StartTime = activeShift.StartTime,
                StartingCash = activeShift.StartingCash,
                TotalGamingIncome = gamingIncome,
                TotalBuffetIncome = buffetIncome,
                TotalDebtCollected = debtCollected,
                TotalExpenses = totalExpenses,
                   ActualCash= expectedCash,
                Notes = activeShift.Notes
            };
        }

    }
}
