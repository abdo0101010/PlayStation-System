using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Customer
    {
        public string Id { get; set; }=Guid.NewGuid().ToString();

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty;

        [MaxLength(15)]
        public string? Phone { get; set; }

        public int TotalPoints { get; set; } = 0; // نقاط الولاء للخصومات

        [Column(TypeName = "decimal(18,2)")]
        public decimal Debt { get; set; } = 0; // لو عليه فلوس نوتة/آجل
    }
}
