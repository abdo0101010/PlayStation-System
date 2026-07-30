namespace PlaystationSystem.ViewModel
{
    public class PricingDeviceViewMOdel
    {
        public int DeviceId { get; set; }
        public string ?DeviceName { get; set; } 
        public decimal HourPriceSingle { get; set; }
        public decimal HourPriceMulti { get; set; }
        public string Type { get; set; } = string.Empty;
        public bool IsOccupied { get; set; }  
    }
}
