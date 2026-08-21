using Microsoft.AspNetCore.Identity;
using PlaystationSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.Models
{
    public class ApplicationUser:IdentityUser, ITenantEntity
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty; // اسم الكاشير / المستخدم
        public string TenantId { get; set; } = string.Empty;
        public bool IsActive { get; set; } = true; // حالة الحساب (نشط / معطل)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Monthly;
        public DateTime? SubscriptionStartDate { get; set; }
        public DateTime? SubscriptionEndDate { get; set; }

        // 💡 نقل علاقة الـ Sessions لكلاس Identity الرئيسي
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

    }
}
