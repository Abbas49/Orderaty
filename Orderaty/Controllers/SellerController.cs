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

        // --------------------- 📊 Dashboard ---------------------
        public async Task<IActionResult> Dashboard()
        {
            var user = await userManager.GetUserAsync(User);
            var seller = await db.Sellers
                .Include(s => s.User)
                .Include(s => s.Products)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.OrderedItems)
                        .ThenInclude(oi => oi.Product)
                .Include(s => s.Orders)
                    .ThenInclude(o => o.Client)
                        .ThenInclude(c => c.User)
                .Include(s => s.SellerReviews)
                    .ThenInclude(sr => sr.Client)
                        .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(s => s.Id == user.Id);

            if (seller == null)
                return RedirectToAction("Login", "User");

            // Calculate statistics
            ViewBag.TotalProducts = seller.Products.Count;
            ViewBag.TotalOrders = seller.Orders.Count;
            ViewBag.PendingOrders = seller.Orders.Count(o => o.Status == OrderStatus.PendingDelivery || o.Status == OrderStatus.Processing);
            ViewBag.TotalRevenue = seller.Orders.Where(o => o.Status == OrderStatus.Delivered).Sum(o => o.TotalPrice);
            ViewBag.AverageRating = seller.SellerReviews.Any() ? seller.SellerReviews.Average(r => r.Rating) : 0;
            ViewBag.TotalReviews = seller.SellerReviews.Count;
            
            // Low stock products (less than 10 items)
            ViewBag.LowStockProducts = seller.Products.Where(p => p.Available_Amount < 10).Count();
            
            // Recent orders (last 5)
            ViewBag.RecentOrders = seller.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToList();

            // Top products by orders
            ViewBag.TopProducts = seller.Products
                .OrderByDescending(p => p.OrderedItems.Sum(oi => oi.Quantity))
                .Take(5)
                .ToList();

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

        // --------------------- ✏️ View All Stores for clients ---------------------
        public async Task<IActionResult> Browse(string? sellerName, string? category, string? sort)
        {
            var sellers = db.Sellers
                .Include(p => p.User)
                .Include(p => p.Products)
                .AsQueryable();

            // البحث بالاسم
            if (!string.IsNullOrEmpty(sellerName))
                sellers = sellers.Where(p => p.User.FullName.Contains(sellerName));

            // فلترة بالفئة (Category)
            if (!string.IsNullOrEmpty(category) && category != "all")
            {
                if (Enum.TryParse<SellerCategory>(category, out var categoryEnum))
                {
                    sellers = sellers.Where(p => p.Category == categoryEnum);
                }
            }

            // الترتيب
            sellers = sort switch
            {
                "rating_desc" => sellers.OrderByDescending(p => p.Rating),
                "rating_asc" => sellers.OrderBy(p => p.Rating),
                "name_asc" => sellers.OrderBy(p => p.User.FullName),
                "name_desc" => sellers.OrderByDescending(p => p.User.FullName),
                _ => sellers.OrderByDescending(p => p.Rating)
            };

            var result = await sellers.ToListAsync();
            
            // Pass the filter values to the view for maintaining state
            ViewBag.CurrentSearch = sellerName;
            ViewBag.CurrentCategory = category;
            ViewBag.CurrentSort = sort;
            
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
