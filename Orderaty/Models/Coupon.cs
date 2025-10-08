namespace Orderaty.Models
{
    public class Coupon
    {
        public int Id { get; set; }
        public string Code { get; set; }
        public bool IsActive { get; set; }
        public DateTime ExpireDate { get; set; }
        public decimal MinimumTotal { get; set; }
        public decimal DiscountValue { get; set; }

        // Relationships
        public List<Order> Orders { get; set; }
    }
}
