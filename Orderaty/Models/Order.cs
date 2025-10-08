namespace Orderaty.Models
{
    public enum OrderStatus
    {
        PendingDelivery,
        Processing,
        Shipped,
        Delivered,
        Cancelled
    }
    public class Order
    {
        public int Id { get; set; }
        public OrderStatus Status { get; set; }
        public DateTime CreatedAt { get; set; }
        public decimal TotalPrice { get; set; }

        // Relationships
        public string ClientId { get; set; }
        public Client Client { get; set; }
        public string DeliveryId { get; set; }
        public Delivery Delivery { get; set; }
        public int CouponId { get; set; }
        public Coupon Coupon { get; set; }
        public List<OrderedItem> OrderedItems { get; set; }
    }
}
