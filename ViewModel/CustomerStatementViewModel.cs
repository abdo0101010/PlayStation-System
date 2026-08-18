namespace PlaystationSystem.ViewModel
{
    public class CustomerStatementViewModel
    {
        public string CustomerId { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string? PhoneNumber { get; set; }
        public decimal CurrentDebt { get; set; }
        public int Points { get; set; }

        public List<CustomerTransactionViewModel> Transactions { get; set; } = new();
    }
}
