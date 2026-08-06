using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.ViewModel
{
    public class LoginVIewModel
    {
        [Required(ErrorMessage = "الاسم بالكامل مطلوب")]
        [Display(Name = "الاسم بالكامل")]
        public string  UserName{ get; set; } = string.Empty;
        [Required(ErrorMessage = "كلمة المرور مطلوبة")]
        [DataType(DataType.Password)]
        [StringLength(100, ErrorMessage = "يجب ألا تقل كلمة المرور عن 6أحرف", MinimumLength = 6)]
        [Display(Name = "كلمة المرور")]
        public string Password { get; set; } = string.Empty;
        [Display(Name = "تذكرني")]
      public  bool RememberMe { get; set; } = false;

    }

}
