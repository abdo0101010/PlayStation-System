using System.ComponentModel.DataAnnotations.Schema;

namespace PlaystationSystem.Models
{
    public class DebtPayment: ITenantEntity
    {
        public int Id { get; set; }
        public string CustomerId { get; set; } = null!;
        [ForeignKey("CustomerId")]
        public virtual Customer? Customer {  get; set;}
        public decimal Amount { get; set; }
        public DateTime PaymentDate { get; set; } = DateTime.Now;
        public string? ShiftId { get; set; }
        public string TenantId { get; set; } = string.Empty;
    }
}
