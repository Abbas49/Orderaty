using Orderaty.Models;
using System.ComponentModel.DataAnnotations;

namespace Orderaty.ViewModels
{
    public class RegisterSeller
    {
        public string FullName { get; set; }
        public string UserName { get; set; }

        [EmailAddress]
        public string Email { get; set; }
        public string Address { get; set; }
        public string? Description { get; set; }
        public SellerCategory Category { get; set; }
        public SellerStatus Status { get; set; } = SellerStatus.Coming_Soon;
        public IFormFile? Image { get; set; }

        [StringLength(11)]
        [RegularExpression(@"01[0-2]\d{8}|015\d{8}", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }
        public decimal Rating { get; set; }

        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must consist of at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password must contain a combination of upper case characters, lower case characters and digits.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
