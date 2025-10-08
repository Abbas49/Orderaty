using System.ComponentModel.DataAnnotations;

namespace Orderaty.ViewModels
{
    public class LoginUser
    {
        [EmailAddress]
        public string Email { get; set; }
        public string Password { get; set; }
        public bool RememberMe { get; set; }
    }
}
