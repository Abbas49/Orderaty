using System.ComponentModel.DataAnnotations;

namespace Orderaty.ViewModels
{
    public class ClientProfile
    {
        public string FullName { get; set; }
        public string Email { get; set; }
        public string UserName { get; set; }

        [StringLength(11)]
        [RegularExpression(@"01[0-2]\d{8}|015\d{8}", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }
        public string Address { get; set; }
        public string? ImagePath { get; set; }
    }
}
