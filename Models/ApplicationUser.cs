using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.Models
{
    public class ApplicationUser:IdentityUser
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        [MaxLength(100)]
        public string FullName { get; set; } = string.Empty; // اسم الكاشير / المستخدم

        public bool IsActive { get; set; } = true; // حالة الحساب (نشط / معطل)

        public DateTime CreatedAt { get; set; } = DateTime.Now;
     

        // 💡 نقل علاقة الـ Sessions لكلاس Identity الرئيسي
        public ICollection<Session> Sessions { get; set; } = new List<Session>();

    }
}
