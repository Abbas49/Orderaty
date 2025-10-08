using Microsoft.AspNetCore.Identity;
using System.ComponentModel.DataAnnotations.Schema;

namespace Orderaty.Models
{
    public class User : IdentityUser
    {
        public string FullName { get; set; }
        public string? Image { get; set; }


        // Relationships
        public Client Client { get; set; }
        public Seller Seller { get; set; }
        public Delivery Delivery { get; set; }
        public List<Message> SenderMessages { get; set; }
        public List<Message> ReceiverMessages { get; set; }

    }
}
