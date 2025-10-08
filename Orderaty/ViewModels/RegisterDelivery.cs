using System.ComponentModel.DataAnnotations;
using Teamspace.Attributes;

namespace Orderaty.ViewModels
{
    public class RegisterDelivery
    {
        public string FullName { get; set; }
        public string UserName { get; set; }
        public string Email { get; set; }

        [StringLength(11)]
        [RegularExpression(@"01[0-2]\d{8}|015\d{8}", ErrorMessage = "Invalid phone number")]
        public string Phone { get; set; }


        [AllowedExtensions(new string[] { ".jpg", ".jpeg", ".png" })]
        public IFormFile? Image { get; set; }

        [StringLength(255, MinimumLength = 8, ErrorMessage = "Password must consist of at least 8 characters")]
        [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d).{8,}$", ErrorMessage = "Password must contain a combination of upper case characters, lower case characters and digits.")]
        public string Password { get; set; }

        [Compare("Password", ErrorMessage = "The passwords do not match.")]
        public string ConfirmPassword { get; set; }
    }
}
