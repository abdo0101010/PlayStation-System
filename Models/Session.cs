using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Session
    {
        public int Id { get; set; }

        public int DeviceId { get; set; }
        [ForeignKey("DeviceId")]
        public Device Device { get; set; } = null!;

        public int? CustomerId { get; set; } // اختياري لو زبون عابر مش مسجل
        [ForeignKey("CustomerId")]
        public Customer? Customer { get; set; }

        public int ShiftId { get; set; }
        [ForeignKey("ShiftId")]
        public Shifts Shift { get; set; } = null!;

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        public string Mode { get; set; } = "Single"; // Single or Multi (بيحدد سعر الساعة)
        public string SessionType { get; set; } = "Open"; // Open (مفتوح) or Limit (محدد بوقت)
        public int TargetMinutes { get; set; } = 0; // لو محدد بوقت، كام دقيقة؟ (مثلاً 60 دقيقة)

        // الحسابات والخصومات
        [Column(TypeName = "decimal(18,2)")]
        public decimal DeviceCost { get; set; } = 0; // حساب الوقت فقط

        [Column(TypeName = "decimal(18,2)")]
        public decimal ProductsCost { get; set; } = 0; // حساب البوفيه فقط

        [Column(TypeName = "decimal(18,2)")]
        public decimal DiscountAmount { get; set; } = 0; // قيمة الخصم (بالجنيه)

        public string? DiscountReason { get; set; } // سبب الخصم (نقط، كود، يدوي)

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalAmount { get; set; } = 0; // الإجمالي النهائي = (الوقت + البوفيه) - الخصم

        public bool IsPaid { get; set; } = false;

        // العلاقات
        public ICollection<SessionOrder> Orders { get; set; } = new List<SessionOrder>();

        public int UserId { get; set; }
        public User User { get; set; } = null!;

    }
}
