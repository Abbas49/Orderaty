namespace Orderaty.Models
{
    public class Message
    {
        public int Id { get; set; }

        // Relationships
        public string SenderId { get; set; }
        public User Sender { get; set; }
        public string ReceiverId { get; set; }
        public User Receiver { get; set; }
    }
}
