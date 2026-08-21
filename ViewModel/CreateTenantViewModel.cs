using PlaystationSystem.Enums;
using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.ViewModel
{
    public class CreateTenantViewModel
    {
        // بيانات المحل / الفرع
        [Required(ErrorMessage = "اسم المحل مطلوب")]
        [Display(Name = "اسم صالة البلايستيشن")]
        public string StoreName { get; set; } = null!;

        [Required(ErrorMessage = "اسم المالك مطلوب")]
        [Display(Name = "اسم صاحب المحل")]
        public string OwnerName { get; set; } = null!;

        [Required(ErrorMessage = "رقم الهاتف مطلوب")]
        [Display(Name = "رقم الهاتف")]
        public string Phone { get; set; } = null!;

        [Display(Name = "نوع باقة الاشتراك")]
        public SubscriptionType SubscriptionType { get; set; } = SubscriptionType.Monthly;

        [Display(Name = "مدة الاشتراك بالشهور (في حالة الشهري)")]
        public int DurationInMonths { get; set; } = 1;

        // بيانات حساب الأدمن التابع للمحل
        [Required(ErrorMessage = "اسم المستخدم مطلوب")]
        [Display(Name = "اسم مستخدم الأدمن")]
        public string UserName { get; set; } = null!;

        [Required(ErrorMessage = "البريد الإلكتروني مطلوب")]
        [EmailAddress(ErrorMessage = "بريد إلكتروني غير صالح")]
        public string Email { get; set; } = null!;

        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = null!;
    }
}
