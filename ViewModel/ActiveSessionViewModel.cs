namespace PlaystationSystem.ViewModel
{
    public class ActiveSessionViewModel
    {
        public string SessionId { get; set; } = string.Empty;
        public string DeviceId { get; set; } = string.Empty;
        public string DeviceName { get; set; } = string.Empty;
        public string DeviceType { get; set; } = string.Empty;
        public string CustomerName { get; set; } = string.Empty;
        public DateTime StartTime { get; set; }
        public string Mode { get; set; } = "Single"; // Single / Multi
        public string SessionType { get; set; } = "Open"; // Open / Limit
        public int TargetMinutes { get; set; }
        public decimal HourPriceSingle { get; set; }
        public decimal HourPriceMulti { get; set; }
        public decimal ProductsCost { get; set; }
    }
}
