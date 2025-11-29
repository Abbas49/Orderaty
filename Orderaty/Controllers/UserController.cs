using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.WebUtilities;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;
using System.Linq;
using System.Security.Claims;
using System.Text;

namespace Orderaty.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IWebHostEnvironment hostingEnvironment;
        private readonly IEmailSender _emailSender;
        public UserController(AppDbContext db, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment hostingEnvironment, IEmailSender emailSender)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.hostingEnvironment = hostingEnvironment;
            _emailSender = emailSender;
        }

        public IActionResult Login()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                var userData = await userManager.FindByEmailAsync(user.Email);
                if (userData == null)
                {
                    ModelState.AddModelError(string.Empty, "Invalid email or password.");
                    return View(user);
                }
                var result = await signInManager.PasswordSignInAsync(
                    userData.UserName,
                    user.Password,
                    user.RememberMe,
                    false
                );
                if (result.Succeeded)
                {
                    if (await userManager.IsInRoleAsync(userData, "Admin"))
                    {
                        return RedirectToAction("Dashboard", "Admin");
                    }
                    if (await userManager.IsInRoleAsync(userData, "Seller"))
                    {
                        return RedirectToAction("Dashboard", "Seller");
                    }
                    if (await userManager.IsInRoleAsync(userData, "Delivery"))
                    {
                        return RedirectToAction("Dashboard", "Delivery");
                    }

                    return RedirectToAction("Index", "Home");
                }
                ModelState.AddModelError(string.Empty, "Invalid Login Attempt Try Again");
            }
            return View(user);
        }


        public IActionResult Register()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser _user)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = _user.UserName,
                    FullName = _user.FullName,
                    Email = _user.Email,
                    PhoneNumber = _user.Phone,
                };
                var result = await userManager.CreateAsync(user, _user.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRole.Client.ToString());
                    await userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
                    var client = new Client
                    {
                        Id = user.Id,
                        Address = _user.Address
                    };


                    if (_user.Image != null)
                        user.Image = await SaveImage(_user.Image);


                    await db.Clients.AddAsync(client);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Login");
                }
                else
                {
                    foreach (var error in result.Errors)
                    {
                        ModelState.AddModelError(string.Empty, error.Description);
                    }
                    return View(_user);
                }
            }
            return View(_user);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AddSeller()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddSeller(RegisterSeller _user)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = _user.UserName,
                    FullName = _user.FullName,
                    Email = _user.Email,
                    PhoneNumber = _user.Phone,
                };
                var result = await userManager.CreateAsync(user, _user.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRole.Seller.ToString());
                    await userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
                    var seller = new Seller
                    {
                        Id = user.Id,
                        Address = _user.Address,
                        Description = _user.Description,
                        Category = _user.Category,
                        Status = _user.Status,
                        Rating = _user.Rating
                    };

                    if (_user.Image != null)
                        user.Image = await SaveImage(_user.Image);

                    await db.Sellers.AddAsync(seller);
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Seller '{user.FullName}' has been successfully added.";
                    return RedirectToAction("Users", "Admin");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Register Attempt Try Again");
                    return View(_user);
                }
            }
            return View(_user);
        }


        [Authorize(Roles = "Admin")]
        public IActionResult AddDelivery()
        {
            return View();
        }

        [HttpPost]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AddDelivery(RegisterDelivery _user)
        {
            if (ModelState.IsValid)
            {
                var user = new User
                {
                    UserName = _user.UserName,
                    FullName = _user.FullName,
                    Email = _user.Email,
                    PhoneNumber = _user.Phone,
                };
                var result = await userManager.CreateAsync(user, _user.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRole.Delivery.ToString());
                    await userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
                    var delivery = new Delivery
                    {
                        Id = user.Id
                    };

                    if (_user.Image != null)
                        user.Image = await SaveImage(_user.Image);

                    await db.Deliveries.AddAsync(delivery);
                    await db.SaveChangesAsync();
                    TempData["SuccessMessage"] = $"Delivery personnel '{user.FullName}' has been successfully added.";
                    return RedirectToAction("Users", "Admin");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Register Attempt Try Again");
                    return View(_user);
                }
            }
            return View(_user);
        }
        public async Task<IActionResult> Logout()
        {
            await signInManager.SignOutAsync();
            return RedirectToAction("Login");
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var imgPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "users");
            if (!Directory.Exists(imgPath))
            {
                Directory.CreateDirectory(imgPath);
            }
            imgPath = Path.Combine(imgPath, fileName);
            using (var fileStream = new FileStream(imgPath, FileMode.Create))
            {
                await image.CopyToAsync(fileStream);
            }
            return fileName;
        }

        [HttpGet]
        public IActionResult ForgotPassword()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null)
            {
                TempData["InboxUrl"] = GetMailInboxUrl(model.Email);
                return RedirectToAction("ForgotPasswordConfirmation");
            }

            var token = await userManager.GeneratePasswordResetTokenAsync(user);
            var encodedToken = WebEncoders.Base64UrlEncode(Encoding.UTF8.GetBytes(token));
            var resetLink = Url.Action("ResetPassword", "User", new { email = model.Email, token = encodedToken }, Request.Scheme);

            await _emailSender.SendEmailAsync(model.Email, "Reset Password", $"Click here to reset your password: <a href='{resetLink}'>Reset Password</a>");

            TempData["InboxUrl"] = GetMailInboxUrl(model.Email);

            return RedirectToAction("ForgotPasswordConfirmation");
        }

        [HttpGet]
        public IActionResult ForgotPasswordConfirmation()
        {
            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token, string email)
        {
            if (token == null || email == null) return BadRequest("Invalid password reset request.");

            var model = new ResetPasswordViewModel { Token = token, Email = email };
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var user = await userManager.FindByEmailAsync(model.Email);
            if (user == null) return RedirectToAction("ResetPasswordConfirmation");

            var decodedBytes = WebEncoders.Base64UrlDecode(model.Token);
            var decodedToken = Encoding.UTF8.GetString(decodedBytes);

            var result = await userManager.ResetPasswordAsync(user, decodedToken, model.Password);
            if (result.Succeeded)
                return RedirectToAction("ResetPasswordConfirmation");

            foreach (var error in result.Errors)
                ModelState.AddModelError("", error.Description);

            return View(model);
        }

        [HttpGet]
        public IActionResult ResetPasswordConfirmation()
        {
            return View();
        }

        private string GetMailInboxUrl(string email)
        {
            if (string.IsNullOrWhiteSpace(email) || !email.Contains("@"))
                return "mailto:";

            var domain = email.Split('@').Last().ToLowerInvariant();

            return domain switch
            {
                "gmail.com" or "googlemail.com" => "https://mail.google.com/mail/u/0/#inbox",
                "yahoo.com" or "yahoo.eg" => "https://mail.yahoo.com",
                "outlook.com" or "hotmail.com" or "live.com" or "msn.com" => "https://outlook.live.com/mail/inbox",
                "icloud.com" or "me.com" or "mac.com" => "https://www.icloud.com/mail",
                "aol.com" => "https://mail.aol.com/webmail",
                "yandex.com" or "yandex.ru" => "https://mail.yandex.com",
                _ => $"mailto:{email}"
            };
        }
    }
}
