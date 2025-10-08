namespace Orderaty.Models
{
    public class SellerReview
    {
        public int Id { get; set; }
        public decimal Rating { get; set; }
        public string? Comment { get; set; }

        // Relationships
        public string ClientId { get; set; }
        public Client Client { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
