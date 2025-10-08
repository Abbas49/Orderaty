namespace Orderaty.Models
{
    public class Product
    {
        public int Id { get; set; }
        public string Name { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }
        public decimal Rating { get; set; }
        public decimal Price { get; set; }

        // Relationships
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public List<OrderedItem> OrderedItems { get; set; }
    }
}
