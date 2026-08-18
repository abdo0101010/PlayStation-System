using PlaystationSystem.Models;

namespace PlaystationSystem.ViewModel
{
    internal class SessionDetailsViewModel
    {
        public Session Session { get; set; }
        public string DeviceName { get; set; }
        public string? CustomerName { get; set; }
    }
}