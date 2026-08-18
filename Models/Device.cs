using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Device
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(50)]
        public string Name { get; set; } = string.Empty; // مثلاً: جهاز 1 - جهاز 2 VIP

        [Required]
        public string Type { get; set; } = "Normal"; // Normal, VIP, VR

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourPriceSingle { get; set; } // سعر الساعة دراع واحد

        [Column(TypeName = "decimal(18,2)")]
        public decimal HourPriceMulti { get; set; } // سعر الساعة دراعين أو أكتر

        public bool IsActive { get; set; } = true; // متاح حالياً ولا عطلان
    }
}
