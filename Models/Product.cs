using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class Product
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();

        [Required, MaxLength(100)]
        public string Name { get; set; } = string.Empty; // شيبسي، كانز، قهوة...

        [Column(TypeName = "decimal(18,2)")]
        public decimal PurchasePrice { get; set; } // سعر الشراء (لحساب صافي الأرباح)

        [Column(TypeName = "decimal(18,2)")]
        public decimal SellingPrice { get; set; } // سعر البيع للزبون

        public int StockQuantity { get; set; }
    }
}
