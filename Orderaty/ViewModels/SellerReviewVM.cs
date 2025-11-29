using System.ComponentModel.DataAnnotations;

namespace Orderaty.ViewModels
{
    public class SellerReviewVM
    {
        [Required(ErrorMessage = "Rating is required")]
        [Range(1, 5, ErrorMessage = "Rating must be between 1 and 5")]
        public decimal Rating { get; set; }

        [StringLength(500, ErrorMessage = "Comment cannot exceed 500 characters")]
        public string? Comment { get; set; }

        [Required]
        public string SellerId { get; set; }
    }
}

