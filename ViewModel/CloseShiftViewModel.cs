namespace PlaystationSystem.ViewModel
{
    public class CloseShiftViewModel
    {
        public string ShiftId { get; set; }
        public string CashierName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public decimal StartingCash { get; set; }

        public decimal TotalGamingIncome { get; set; }
        public decimal TotalBuffetIncome { get; set; }
        public decimal TotalDebtCollected { get; set; }
        public decimal TotalExpenses { get; set; }

        // المبلغ المتوقع دفترياً في الدرج
        public decimal ExpectedCash => (StartingCash + TotalGamingIncome + TotalBuffetIncome + TotalDebtCollected) - TotalExpenses;

        // المبلغ الفعلي الذي يعده الكاشير
        public decimal ActualCash { get; set; }
        public string? Notes { get; set; }
    }
}
