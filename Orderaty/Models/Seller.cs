using System.ComponentModel.DataAnnotations.Schema;

namespace Orderaty.Models
{
    public enum SellerStatus
    {
        Open,
        Closed,
        Coming_Soon
    }
    public enum SellerCategory
    {
        Food_Restaurants,
        Groceries_Supermarkets,
        Pharmacy_Health,
        Electronics_Mobile,
        Fashion_Clothing,
        Home_Furniture,
        Stationery_Books,
        Sports_Fitness,
        Flowers_Gifts,
        Automotive,
        Pets_Animals,
        Other_Services
    }
    public class Seller
    {
        [ForeignKey("User")]
        public string Id { get; set; }
        public string? Description { get; set; }
        public string Address { get; set; }
        public SellerStatus Status { get; set; }
        public SellerCategory Category { get; set; }
        public decimal Rating { get; set; }

        // Relationships
        public User User { get; set; }
        public List<Product> Products { get; set; }
        public List<OrderedItem> OrderedItems { get; set; }
    }
}
