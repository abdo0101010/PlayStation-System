using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.Models
{
    public class User
    {
        public int Id { get; set; }
        [Required]

        public string Username { get; set; }
        [Required]
        [Phone(ErrorMessage = "Invalid phone number format.")]
        
        public string PhoneNumber { get; set; }
        [Required]
        public string Password { get; set; }
        [Required]
        public string Role { get; set; }
        public bool IsActive { get; set; } = false;
        public ICollection<Session> Sessions { get; set; } = new List<Session>();
    }
}
