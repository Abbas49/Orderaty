using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;

namespace Orderaty.Controllers
{
    public class UserController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IWebHostEnvironment hostingEnvironment;
        public UserController(AppDbContext db, UserManager<User> userManager, SignInManager<User> signInManager, IWebHostEnvironment hostingEnvironment)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        public IActionResult Login()
        {
            return View(new LoginUser());
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginUser user)
        {
            if (ModelState.IsValid)
            {
                var userData = await userManager.FindByEmailAsync(user.Email);
                if (userData != null && userData.UserName != null)
                {
                    var result = await signInManager.PasswordSignInAsync(userData.UserName, user.Password, user.RememberMe, false);
                    if (result.Succeeded)
                    {
                        return RedirectToAction("Index", "Home");
                    }
                    else
                    {
                        ModelState.AddModelError(string.Empty, "Invalid Login Attempt Try Again");
                        return View(user);
                    }
                }
                
            }
            return View(user);
        }

        public IActionResult Register()
        {
            return View(new RegisterUser());
        }

        [HttpPost]
        public async Task<IActionResult> Register(RegisterUser _user)
        {
            if (ModelState.IsValid) 
            {
                var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "users");
                var user = new User
                {
                    UserName = _user.UserName,
                    FullName = _user.FullName,
                    Email = _user.Email,
                    PhoneNumber = _user.Phone,
                    Image = _user.Image != null ? Path.Combine(hostingEnvironment.WebRootPath, "images", "users", _user.Image.FileName) : null
                };
                var result = await userManager.CreateAsync(user, _user.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRole.Client.ToString());
                    if (_user.Image != null && user.Image != null)
                    {
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        _user.Image.CopyTo(new FileStream(user.Image, FileMode.Create));
                    }

                    var client = new Client
                    {
                        Id = user.Id,
                        Address = _user.Address
                    };
                    await db.Clients.AddAsync(client);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Login");
                }
                else
                {
                    ModelState.AddModelError(string.Empty, "Invalid Register Attempt Try Again");
                    return View(_user);
                }
            }
            return View(_user);
        }


        public IActionResult AddSeller()
        {
            return View(new RegisterSeller());
        }

        [HttpPost]
        public async Task<IActionResult> AddSeller(RegisterSeller _user)
        {
            if (ModelState.IsValid)
            {
                var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "users");
                var user = new User
                {
                    UserName = _user.UserName,
                    FullName = _user.FullName,
                    Email = _user.Email,
                    PhoneNumber = _user.Phone,
                    Image = _user.Image != null ? Path.Combine(hostingEnvironment.WebRootPath, "images", "users", _user.Image.FileName) : null
                };
                var result = await userManager.CreateAsync(user, _user.Password);
                if (result.Succeeded)
                {
                    await userManager.AddToRoleAsync(user, UserRole.Seller.ToString());
                    if (_user.Image != null && user.Image != null)
                    {
                        if (!Directory.Exists(folderPath))
                        {
                            Directory.CreateDirectory(folderPath);
                        }
                        _user.Image.CopyTo(new FileStream(user.Image, FileMode.Create));
                    }

                    var seller = new Seller
                    {
                        Id = user.Id,
                        Address = _user.Address,
                        Description = _user.Description,
                        Category = _user.Category,
                        Status = _user.Status,
                        Rating = _user.Rating
                    };
                    await db.Sellers.AddAsync(seller);
                    await db.SaveChangesAsync();
                    return RedirectToAction("Login");
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
    }
}
