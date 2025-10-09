using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    [Authorize(Roles = "Seller")]
    public class SellerController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly IWebHostEnvironment hostingEnvironment;

        public SellerController(AppDbContext db, UserManager<User> userManager, IWebHostEnvironment hostingEnvironment)
        {
            this.db = db;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        // --------------------- 🏠 Home ---------------------
        public async Task<IActionResult> Home()
        {
            var user = await userManager.GetUserAsync(User);
            var seller = await db.Sellers.Include(s => s.User)
                                         .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (seller == null)
                return RedirectToAction("Login", "User");

            return View(seller);
        }

        // --------------------- 👤 Profile ---------------------
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            var seller = await db.Sellers.Include(s => s.User)
                                         .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (seller == null)
                return NotFound();

            return View(seller);
        }

        // --------------------- ✏️ Edit ---------------------
        [HttpGet]
        public async Task<IActionResult> Edit()
        {
            var user = await userManager.GetUserAsync(User);
            var seller = await db.Sellers.Include(s => s.User)
                                         .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (seller == null)
                return NotFound();

            return View(seller);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Seller model, IFormFile? imageFile)
        {
            if (ModelState.IsValid)
            {
                var seller = await db.Sellers.Include(s => s.User)
                                             .FirstOrDefaultAsync(s => s.Id == model.Id);

                if (seller == null)
                    return NotFound();

                // تحديث بيانات البائع
                seller.Description = model.Description;
                seller.Address = model.Address;
                seller.Status = model.Status;
                seller.Category = model.Category;

                // تحديث بيانات المستخدم (الاسم والصورة)
                seller.User.FullName = model.User.FullName;

                if (imageFile != null)
                {
                    var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "users");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var imagePath = Path.Combine(folderPath, imageFile.FileName);
                    using (var stream = new FileStream(imagePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }
                    seller.User.Image = Path.Combine("images", "users", imageFile.FileName);
                }

                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Profile));
            }

            return View(model);
        }
    }
}
