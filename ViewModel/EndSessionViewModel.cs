namespace PlaystationSystem.ViewModel
{
    public class EndSessionViewModel
    {
        public string SessionId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public TimeSpan Duration { get; set; }
        public string Mode { get; set; } = "Single";
        public decimal HourlyRate { get; set; }
        public decimal DeviceCost { get; set; }

        // قائمة المنتجات المتاحة للاختيار منها
        public List<ProductItemViewModel> AvailableProducts { get; set; } = new();

        // قائمة المنتجات التي اختارها العميل (تُرسل عند الـ Submit)
        public List<SelectedOrderItem> SelectedOrders { get; set; } = new();

        public decimal DiscountAmount { get; set; }
    }
}
