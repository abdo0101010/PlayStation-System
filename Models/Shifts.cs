using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Shifts
    {
        public int Id { get; set; }

        public string UserId { get; set; }

        public DateTime StartTime { get; set; } = DateTime.Now;
        public DateTime? EndTime { get; set; }

        [Column(TypeName = "decimal(18,2)")]
        public decimal StartingCash { get; set; } // الفلوس اللي بيبدأ بيها الوردية (الدرج)

        [Column(TypeName = "decimal(18,2)")]
        public decimal ExpectedCash { get; set; } // الحسابات التلقائية (الدرج + المبيعات والوقت)

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ActualCash { get; set; } // الكاشير لقى كام فعلياً في الدرج وهو بيقفل

        [Column(TypeName = "decimal(18,2)")]
        public decimal? ShortageOrSurplus { get; set; } // العجز أو الزيادة
        [ForeignKey("UserId")]
        public ApplicationUser User { get; set; } = null!;
        //// Scaffold-DbContext "Server=.\SQLEXPRESS;Database=PlaystationSystem;Trusted_Connection=True;"Microsoft.EntityFrameworkCore.SqlServer - OutputDir Models
    }
}
