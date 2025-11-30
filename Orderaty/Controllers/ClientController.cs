using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;
using System;
using System.IO;
using System.Linq;
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
            if (User.Identity == null)
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

        // GET: /Client/EditProfile
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            if (User.Identity == null)
                return RedirectToAction("Logout", "User");
            if (!User.Identity.IsAuthenticated)
                return RedirectToAction("Logout", "User");

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Logout", "User");

            var clientEntity = await db.Clients.FirstOrDefaultAsync(c => c.Id == user.Id);

            var profile = new ClientProfile
            {
                FullName = user.FullName,
                Email = user.Email,
                UserName = user.UserName,
                Phone = user.PhoneNumber,
                Address = clientEntity?.Address,
                ImagePath = user.Image
            };

            return View(profile);
        }

        [HttpPost]
        public async Task<IActionResult> Update(ClientProfile _profile, string? currentPassword, string? newPassword, string? confirmPassword)
        {
            if (_profile == null) return RedirectToAction("Profile");

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Logout", "User");

            // Basic model validation
            if (!ModelState.IsValid)
            {
                TempData["ErrorMessage"] = "Please provide valid profile information.";
                return RedirectToAction("Profile");
            }

            // Update fields
            user.FullName = _profile.FullName;
            user.PhoneNumber = _profile.Phone;
            var clientEntity = await db.Clients.FirstOrDefaultAsync(c => c.Id == user.Id);
            if (clientEntity != null)
            {
                clientEntity.Address = _profile.Address;
            }

            // Password change handling
            var wantsPasswordChange = !string.IsNullOrEmpty(currentPassword) || !string.IsNullOrEmpty(newPassword) || !string.IsNullOrEmpty(confirmPassword);
            if (wantsPasswordChange)
            {
                if (string.IsNullOrEmpty(currentPassword))
                    ModelState.AddModelError("currentPassword", "Current password is required to change password.");
                if (string.IsNullOrEmpty(newPassword))
                    ModelState.AddModelError("newPassword", "New password is required.");
                if (newPassword != confirmPassword)
                    ModelState.AddModelError("confirmPassword", "New password and confirmation do not match.");

                if (!ModelState.IsValid)
                {
                    TempData["ErrorMessage"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage ?? "Invalid password input.";
                    return RedirectToAction("Profile");
                }
            }

            // Update identity fields first
            var updateResult = await userManager.UpdateAsync(user);
            if (!updateResult.Succeeded)
            {
                TempData["ErrorMessage"] = string.Join("; ", updateResult.Errors.Select(e => e.Description));
                return RedirectToAction("Profile");
            }

            // Change password if requested
            if (wantsPasswordChange)
            {
                var changeResult = await userManager.ChangePasswordAsync(user, currentPassword!, newPassword!);
                if (!changeResult.Succeeded)
                {
                    TempData["ErrorMessage"] = string.Join("; ", changeResult.Errors.Select(e => e.Description));
                    return RedirectToAction("Profile");
                }
            }

            // Persist client entity changes
            await db.SaveChangesAsync();

            // refresh sign-in to update claims/cookies
            await signInManager.RefreshSignInAsync(user);

            TempData["SuccessMessage"] = "Profile updated successfully.";
            return RedirectToAction("Profile");
        }

        [HttpPost]
        public async Task<IActionResult> ChangePhoto(IFormFile image)
        {
            if (image == null)
                ModelState.AddModelError("Image", "Please select an image.");
            else if (!image.FileName.EndsWith(".jpg") &&
                !image.FileName.EndsWith(".png") &&
                !image.FileName.EndsWith(".jpeg"))
                ModelState.AddModelError("Image", "Only .jpg, .jpeg, .png files are allowed.");
            else
            {
                var user = await userManager.GetUserAsync(User);
                if (user == null)
                    return RedirectToAction("Logout", "User");

                var oldImagePath = user.Image;
                if (oldImagePath != null)
                {
                    var oldImageFullPath = Path.Combine(hostEnvironment.WebRootPath, "images", "users", oldImagePath.TrimStart('/'));
                    if (System.IO.File.Exists(oldImageFullPath))
                    {
                        System.IO.File.Delete(oldImageFullPath);
                    }
                }

                user.Image = await SaveImage(image);
                await userManager.UpdateAsync(user);
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