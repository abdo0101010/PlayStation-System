using System.ComponentModel.DataAnnotations.Schema;
using System.Reflection;

namespace PlaystationSystem.Models
{
    public class SessionOrder
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        public string SessionId { get; set; }
        [ForeignKey("SessionId")]
        public Session Session { get; set; } = null!;

        public string ProductId { get; set; }
        [ForeignKey("ProductId")]
        public Product Product { get; set; } = null!;

        public int Quantity { get; set; } // الكمية المطلوبة

        [Column(TypeName = "decimal(18,2)")]
        public decimal UnitPrice { get; set; }
        [NotMapped]
        public decimal TotalPrice => Quantity * UnitPrice;
    }
}
// Scaffold-DbContext "Server=localdb;Database=PlaystationSystem;Trusted_Connection=True;"Microsoft.EntityFrameworkCore.SqlServer - OutputDir Models