using System.ComponentModel.DataAnnotations.Schema;

namespace Orderaty.Models
{
    public class Client
    {
        [ForeignKey("User")]
        public string Id { get; set; }
        public string Address { get; set; }

        // Relationships
        public User User { get; set; }
        public List<Order> Orders { get; set; }
        public List<CartItem> CartItems { get; set; }
        public List<SellerReview> SellerReviews { get; set; }
        //public List<ProductReview> ProductReviews { get; set; }  ---> Error adding product
    }
}
