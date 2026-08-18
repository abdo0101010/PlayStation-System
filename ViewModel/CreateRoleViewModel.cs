using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.ViewModel
{
    public class CreateRoleViewModel
    {
        [Required(ErrorMessage = "اسم الرول مطلوب")]
        [Display(Name = "اسم الرول")]
        public string RoleName { get; set; } = string.Empty;
    }
}
