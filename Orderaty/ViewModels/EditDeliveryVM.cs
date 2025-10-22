using System.ComponentModel.DataAnnotations;

namespace Orderaty.ViewModels
{
    public class EditDeliveryVM
    {
        [Required]
        [Display(Name = "Full Name")]
        public string FullName { get; set; }

        [Phone]
        [Display(Name = "Phone Number")]
        public string PhoneNumber { get; set; }

        public string? CurrentImage { get; set; }

        [Display(Name = "New Profile Image")]
        public IFormFile? NewImage { get; set; }
    }
}
