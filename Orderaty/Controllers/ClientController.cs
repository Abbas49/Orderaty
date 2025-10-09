using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting.Internal;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;
using System.Security.Claims;
using System.Threading.Tasks;

namespace Orderaty.Controllers
{
    public class ClientController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly SignInManager<User> signInManager;
        private readonly IWebHostEnvironment hostEnvironment;
        public ClientController(AppDbContext db, UserManager<User> userManager, 
            SignInManager<User> signInManager, IWebHostEnvironment hostEnvironment)
        {
            this.db = db;
            this.userManager = userManager;
            this.signInManager = signInManager;
            this.hostEnvironment = hostEnvironment;
        }
        public IActionResult Profile()
        {
            if(User.Identity == null)
                return RedirectToAction("Logout", "User");
            if (User.Identity.IsAuthenticated)
            {
                var userName = User.Identity.Name;
                var client = db.Users.Include(c => c.Client).FirstOrDefault(u => u.UserName == userName);

                if (client == null)
                    return RedirectToAction("Logout", "User");

                var profile = new ClientProfile
                {
                    FullName = client.FullName,
                    Email = client.Email,
                    UserName = client.UserName,
                    Phone = client.PhoneNumber,
                    Address = client.Client.Address,
                    ImagePath = client.Image
                };
                return View(profile);
            }
            return RedirectToAction("Logout", "User");
        }

        [HttpPost]
        public async Task<IActionResult> Update(ClientProfile _profile)
        {
            if (ModelState.IsValid)
            {
                var user = await db.Users.Include(u => u.Client).Where(u => u.UserName == _profile.UserName).FirstOrDefaultAsync();
                if (user == null)
                    return RedirectToAction("Logout", "User");
                user.FullName = _profile.FullName;
                user.PhoneNumber = _profile.Phone;
                user.Client.Address = _profile.Address;
                await db.SaveChangesAsync();

                var claims = await userManager.GetClaimsAsync(user);
                var oldClaim = claims.FirstOrDefault(c => c.Type == "FullName");
                if (oldClaim != null)
                    await userManager.RemoveClaimAsync(user, oldClaim);

                await userManager.AddClaimAsync(user, new Claim("FullName", user.FullName));
                await signInManager.RefreshSignInAsync(user);
            }
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePhoto(IFormFile image)
        {
            if(image == null)
                ModelState.AddModelError("Image", "Please select an image.");
            else if(!image.FileName.EndsWith(".jpg") && 
                !image.FileName.EndsWith(".png") && 
                !image.FileName.EndsWith(".jpeg"))
                ModelState.AddModelError("Image", "Only .jpg, .jpeg, .png files are allowed.");
            else
            {
                var userName = User.Identity.Name;
                var client = db.Users.FirstOrDefault(u => u.UserName == userName);
                if (client == null)
                    return RedirectToAction("Logout", "User");

                var oldImagePath = client.Image;
                if (oldImagePath != null)
                {
                    var oldImageFullPath = Path.Combine(hostEnvironment.WebRootPath, "images", "users", oldImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImageFullPath))
                    {
                        System.IO.File.Delete(oldImageFullPath);
                    }
                }

                client.Image = await SaveImage(image);
                await db.SaveChangesAsync();
            }
            return RedirectToAction("Profile");
        }

        private async Task<string> SaveImage(IFormFile image)
        {
            var fileName = $"{Guid.NewGuid()}{Path.GetExtension(image.FileName)}";
            var imgPath = Path.Combine(hostEnvironment.WebRootPath, "images", "users");
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
    }
}
