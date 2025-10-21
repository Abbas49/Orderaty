using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    //[Authorize(Roles = "Seller")]
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
            var user = await userManager.GetUserAsync(User);
            var seller = await db.Sellers.Include(s => s.User)
                                         .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (seller == null)
                return NotFound();

            // تحديث بيانات البائع
            seller.Description = model.Description;
            seller.Address = model.Address;
            seller.Status = model.Status;
            seller.Category = model.Category;

            // تحديث اسم المستخدم
            if (!string.IsNullOrEmpty(model.User?.FullName))
                seller.User.FullName = model.User.FullName;

            // تحديث الصورة لو اترفعت جديدة
            if (imageFile != null)
            {
                var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "users");
                if (!Directory.Exists(folderPath))
                    Directory.CreateDirectory(folderPath);

                var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                var imagePath = Path.Combine(folderPath, fileName);

                using (var stream = new FileStream(imagePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(stream);
                }

                seller.User.Image = fileName; 
            }

            await db.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        // --------------------- ✏️ View All Sellers for clients ---------------------
        public async Task<IActionResult> Browse(string? sellerName, string? category, string? sort)
        {
            var products = db.Sellers
                .Include(p => p.User)
                .AsQueryable();

            // البحث بالاسم
            if (!string.IsNullOrEmpty(sellerName))
                products = products.Where(p => p.User.FullName.Contains(sellerName));

            // فلترة بالفئة (Category)
            if (!string.IsNullOrEmpty(category))
            {
                if (Enum.TryParse<SellerCategory>(category, out var categoryEnum))
                {
                    products = products.Where(p => p.Category == categoryEnum);
                }
            }

            // الترتيب
            products = sort switch
            {
                "rating_desc" => products.OrderByDescending(p => p.Rating),
                "rating_asc" => products.OrderBy(p => p.Rating),
                _ => products.OrderByDescending(p => p.Id)
            };

            var result = await products.ToListAsync();
            return View(result);
        }

        // --------------------- ✏️ details about store and his products ---------------------
        public IActionResult Details(string id)
        {
            var seller = db.Sellers.Include(s => s.Products).Include(s => s.User)
                .Where(s => s.Id == id).FirstOrDefault();

            if(seller != null)
            {
                return View(seller);
            }

            return NotFound();
        }
    }
}
