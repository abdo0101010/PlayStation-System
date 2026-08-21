using PlaystationSystem.Enums;

namespace PlaystationSystem.Models
{
    public class Tenant
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Name { get; set; } = null!;           // اسم المحل / الصالة
        public string OwnerName { get; set; } = null!;      // اسم صاحب المحل
        public string Phone { get; set; } = null!;
        public bool IsActive { get; set; } = true;         // تفعيل / تعطيل
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Monthly;
        public DateTime? SubscriptionEndDate { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
