using System.ComponentModel.DataAnnotations.Schema;

namespace Orderaty.Models
{
    public class Delivery
    {
        [ForeignKey("User")]
        public string Id { get; set; }

        // Relationships
        public User User { get; set; }
        public List<Order> Orders { get; set; }
    }
}
