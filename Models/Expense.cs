using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Expense
    {
        public int Id { get; set; }

        public string Title { get; set; } = string.Empty; // بند الصرف (مثلاً: شراء بن وسكر)

        [Column(TypeName = "decimal(18,2)")]
        public decimal Amount { get; set; } // المبلغ المصروف

        public DateTime CreatedAt { get; set; } = DateTime.Now;
        public string? Notes { get; set; }

        // ربط المصروف بالوردية الحالية
        public int ShiftId { get; set; }
        public Shifts Shift { get; set; } = null!;
    }
}
