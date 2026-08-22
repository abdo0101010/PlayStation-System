using Microsoft.EntityFrameworkCore;
using PlaystationSystem.Models;
using PlaystationSystem.ViewModel;

namespace PlaystationSystem.Repositoriy
{
    public class ShiftRepositiory : IShiftRepositiory
    {
        private readonly ApplicationDbContext _context;
        private readonly ICurrentTenantRepositoriy _currentTenantRepository;

        public ShiftRepositiory(ApplicationDbContext context, ICurrentTenantRepositoriy currentTenantRepository)
        {
            _context = context;
            _currentTenantRepository = currentTenantRepository;
        }

        public async Task<List<Shifts>> GetDescShift()
        {
            var currentTenantId = _currentTenantRepository.TenantId;
            var isSuperAdmin = _currentTenantRepository.IsSuperAdmin;

            return await _context.Shifts
                .Include(s => s.User)
                .Where(s => isSuperAdmin || (s.TenantId == currentTenantId && !string.IsNullOrEmpty(currentTenantId)))
                .OrderByDescending(s => s.StartTime)
                .ToListAsync();
        }

        public async Task<Shifts?> GetActiveShiftAsync()
        {
            var currentTenantId = _currentTenantRepository.TenantId;
            var isSuperAdmin = _currentTenantRepository.IsSuperAdmin;

            return await _context.Shifts
                .Include(s => s.User)
                .SingleOrDefaultAsync(s => s.IsOpen && (isSuperAdmin || s.TenantId == currentTenantId));
        }

        public async Task<CloseShiftViewModel> PrepareCloseShiftSummaryAsync(Shifts activeShift, string cashierName)
        {
            var startTime = activeShift.StartTime;
            var currentTenantId = _currentTenantRepository.TenantId;
            var isSuperAdmin = _currentTenantRepository.IsSuperAdmin;

            // 1. حساب إيراد أجهزة البلايستيشن لنفس الفرع
            decimal gamingIncome = await _context.Sessions
                .Where(s => !s.IsOpen && s.EndTime >= startTime && (isSuperAdmin || s.TenantId == currentTenantId))
                .SumAsync(s => (decimal?)s.TotalAmount) ?? 0;

            // 2. حساب مبيعات البوفيه لنفس الفرع
            decimal buffetIncome = await _context.SessionOrders
                .Where(o => !o.Session.IsOpen && o.Session.EndTime >= startTime && (isSuperAdmin || o.Session.TenantId == currentTenantId))
                .SumAsync(o => (decimal?)(o.Quantity * o.UnitPrice)) ?? 0;

            // 3. حساب الديون المحصلة لنفس الفرع
            decimal debtCollected = await _context.DebtPayments
                .Where(p => (p.PaymentDate >= startTime || p.ShiftId == activeShift.Id) && (isSuperAdmin || p.TenantId == currentTenantId))
                .SumAsync(p => (decimal?)p.Amount) ?? 0;

            // 4. حساب المصروفات النثرية لنفس الفرع
            decimal totalExpenses = await _context.Expenses
                .Where(e => (e.CreatedAt >= startTime || e.ShiftId == activeShift.Id) && (isSuperAdmin || e.TenantId == currentTenantId))
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
                ActualCash = expectedCash,
                Notes = activeShift.Notes
            };
        }
    }
}