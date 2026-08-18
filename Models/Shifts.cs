using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Shifts
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string UserId { get; set; } = string.Empty;

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        // حالة الوردية
        public bool IsOpen { get; set; } = true;

        [Column(TypeName = "decimal(18,2)")]
        public decimal StartingCash { get; set; } // الفكة اللي بيبدأ بيها الوردية (الدرج)

        // ================= تفصيل الإيرادات والمصروفات =================
        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalGamingIncome { get; set; } = 0; // إيراد الوقت وأجهزة البلايستيشن

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalBuffetIncome { get; set; } = 0; // مبيعات البوفيه والمشروبات

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalDebtCollected { get; set; } = 0; // تحصيل ديون وسداد نقدية

        [Column(TypeName = "decimal(18,2)")]
        public decimal TotalExpenses { get; set; } = 0; // مصاريف نثرية خرجت من الدرج

        // ================= الإغلاق والجرد =================
        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedCash { get; set; } // الحساب التلقائي المفروض يكون في الدرج

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCash { get; set; } // الكاشير لقى كام فعلياً في الدرج وهو بيقفل

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ShortageOrSurplus { get; set; } // العجز أو الزيادة

        public string? Notes { get; set; } // ملاحظات الكاشير عند الإغلاق

        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
    }
}
