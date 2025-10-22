namespace Orderaty.Models
{
    public class Favourite
    {
        public int Id { get; set; }
        public bool IsFavourite { get; set; }

        // Relationships
        public string ClientId { get; set; }
        public Client Client { get; set; }
        public string SellerId { get; set; }
        public Seller Seller { get; set; }
    }
}
