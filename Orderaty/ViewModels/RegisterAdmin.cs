using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Http;

namespace Orderaty.ViewModels
{
    public class RegisterAdmin
    {
        [Required] public required string FullName { get; set; }
        [Required] public required string UserName { get; set; }
        [Required, EmailAddress] public required string Email { get; set; }
        public string? Phone { get; set; }
        [Required, DataType(DataType.Password)] public required string Password { get; set; }
        [Required, DataType(DataType.Password), Compare("Password")] public required string ConfirmPassword { get; set; }
        public IFormFile? Image { get; set; }
    }
}