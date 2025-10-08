namespace Orderaty.Models
{
    public class ProductReview
    {
        public int Id { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }

        // Relationships
        public string ClientId { get; set; }
        public Client Client { get; set; }
        public int ProductId { get; set; }
        public Product Product { get; set; }
    }
}
