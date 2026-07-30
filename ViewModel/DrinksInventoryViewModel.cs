using System.ComponentModel.DataAnnotations;

namespace PlaystationSystem.ViewModel
{
    public class DrinksInventoryViewModel
    {
        public int Id { get; set; }

        [Required(ErrorMessage = "اسم المنتج مطلوب")]
        [Display(Name = "اسم المنتج / المشروب")]
        public string Name { get; set; } = string.Empty;

        [Required(ErrorMessage = "سعر الشراء مطلوب")]
        [Display(Name = "سعر الشراء (التكلفة)")]
        public decimal PurchasePrice { get; set; }

        [Required(ErrorMessage = "سعر البيع مطلوب")]
        [Display(Name = "سعر البيع")]
        public decimal SellingPrice { get; set; }

        [Required(ErrorMessage = "الكمية مطلوبة")]
        [Display(Name = "الكمية المتاحة")]
        public int StockQuantity { get; set; }

        public bool IsLowStock => StockQuantity <= 5;
    }
}

