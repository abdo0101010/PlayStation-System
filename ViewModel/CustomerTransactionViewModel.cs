namespace PlaystationSystem.ViewModel
{
    public class CustomerTransactionViewModel
    {
        public DateTime Date { get; set; }
        public string TransactionType { get; set; } = string.Empty; // جلسة بلايستيشن / طلبات بوفيه / سداد نقدية
        public string Details { get; set; } = string.Empty;
        public decimal TotalAmount { get; set; } // إجمالي الحساب
        public decimal PaidAmount { get; set; }  // المدفوع
        public decimal RemainingDebt { get; set; } // الآجل (الدين المتبقي من العملية)
    }
}
