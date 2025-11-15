using System.ComponentModel.DataAnnotations;

namespace Orderaty.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        
        [Required(ErrorMessage = "Coupon code is required")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Coupon code must be between 3 and 50 characters")]
        [RegularExpression(@"^[A-Z0-9]+$", ErrorMessage = "Coupon code must contain only uppercase letters and numbers")]
        public string Code { get; set; }
        
        public bool IsActive { get; set; }
        
        [Required(ErrorMessage = "Expiration date is required")]
        [DataType(DataType.Date)]
        [Display(Name = "Expiration Date")]
        public DateTime ExpireDate { get; set; }
        
        [Required(ErrorMessage = "Minimum total is required")]
        [Range(0, double.MaxValue, ErrorMessage = "Minimum total must be a positive value")]
        [Display(Name = "Minimum Order Total")]
        public decimal MinimumTotal { get; set; }
        
        [Required(ErrorMessage = "Discount value is required")]
        [Range(0.01, double.MaxValue, ErrorMessage = "Discount value must be greater than zero")]
        [Display(Name = "Discount Value")]
        public decimal DiscountValue { get; set; }

        // Relationships
        public List<Order>? Orders { get; set; }
    }
}
