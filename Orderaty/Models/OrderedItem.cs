using System.Reflection.PortableExecutable;

namespace Orderaty.Models
{
    public class OrderedItem
    {
        public int Id { get; set; }
        public int Quantity { get; set; }
        public decimal Subtotal { get; set; }

        // Relationships
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
