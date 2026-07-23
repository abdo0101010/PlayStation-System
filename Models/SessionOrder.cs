using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace PlaystationSystem.Models
{
    public class SessionOrder
    {
        public int Id { get; set; }

        public int SessionId { get; set; }
        [ForeignKey("SessionId")]
        public Session Session { get; set; } = null!;

        public int ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } // الكمية المطلوبة

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
    }
}
// Scaffold-DbContext "Server=localdb;Database=PlaystationSystem;Trusted_Connection=True;"Microsoft.EntityFrameworkCore.SqlServer - OutputDir Models