using Microsoft.AspNetCore.Mvc.Rendering;
using PlaystationSystem.Models;
using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.ViewModel
{
    public class StartSessionViewModel
    {
        [Required(ErrorMessage = "برجاء اختيار الجهاز")]
        public string DeviceId { get; set; }

        public string? CustomerId { get; set; } // اختياري (لو زبون غير مسجل يترك فارغاً)

        [Required]
        public string Mode { get; set; } = "Single"; // Single or Multi

        [Required]
        public string SessionType { get; set; } = "Open"; // Open or Limit

        public int TargetMinutes { get; set; } = 0; // لو محدد بوقت

        // قوائم منسدلة للاختيار
        public IEnumerable<Device>? AvailableDevices { get; set; }
        public IEnumerable<Customer>? Customers { get; set; }
    }
}
