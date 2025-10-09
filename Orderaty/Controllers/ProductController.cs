using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    [Authorize(Roles = "Seller")]
    public class ProductController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly IWebHostEnvironment hostingEnvironment;

        public ProductController(AppDbContext db, UserManager<User> userManager, IWebHostEnvironment hostingEnvironment)
        {
            this.db = db;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        // ---------------------- 🧾 Index ----------------------
        // عرض كل المنتجات الخاصة بالبائع الحالي
        public async Task<IActionResult> Index()
        {
            var user = await userManager.GetUserAsync(User);
            var products = await db.Products
                .Where(p => p.SellerId == user.Id)
                .OrderByDescending(p => p.Id)
                .ToListAsync();

            return View(products);
        }

        // ---------------------- 🔍 Details ----------------------
        // عرض تفاصيل منتج معين
        public async Task<IActionResult> Details(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var product = await db.Products
                .Include(p => p.Seller)
                .FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        // ---------------------- ➕ Create ----------------------
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Create(Product model, IFormFile? imageFile)
        {
            var user = await userManager.GetUserAsync(User);

            ModelState.Remove("Seller");
            ModelState.Remove("SellerId");
            ModelState.Remove("OrderedItems");

            if (ModelState.IsValid)
            {
                if (imageFile != null)
                {
                    var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    model.Image = fileName;
                }

                model.SellerId = user.Id;
                model.Rating = 0;

                await db.Products.AddAsync(model);
                await db.SaveChangesAsync();

                return RedirectToAction(nameof(Index));
            }

            return View(model);
        }

        // ---------------------- ✏️ Edit ----------------------
        [HttpGet]
        public async Task<IActionResult> Edit(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            return View(product);
        }

        [HttpPost]
        public async Task<IActionResult> Edit(Product model, IFormFile? imageFile)
        {
            var user = await userManager.GetUserAsync(User);
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == model.Id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();


            ModelState.Remove("Seller");
            ModelState.Remove("SellerId");
            ModelState.Remove("OrderedItems");

            if (ModelState.IsValid)
            {
                product.Name = model.Name;
                product.Description = model.Description;
                product.Price = model.Price;
                product.Available_Amount = model.Available_Amount;

                if (imageFile != null)
                {
                    var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "products");
                    if (!Directory.Exists(folderPath))
                        Directory.CreateDirectory(folderPath);

                    var fileName = $"{Guid.NewGuid()}{Path.GetExtension(imageFile.FileName)}";
                    var filePath = Path.Combine(folderPath, fileName);

                    using (var stream = new FileStream(filePath, FileMode.Create))
                    {
                        await imageFile.CopyToAsync(stream);
                    }

                    product.Image = fileName;
                }

                await db.SaveChangesAsync();
                return RedirectToAction(nameof(Details), new { id = product.Id });
            }

            return View(model);
        }

        // ---------------------- 🗑 Delete ----------------------
        [HttpPost]
        public async Task<IActionResult> Delete(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == id && p.SellerId == user.Id);

            if (product == null)
                return NotFound();

            db.Products.Remove(product);
            await db.SaveChangesAsync();

            return RedirectToAction(nameof(Index));
        }
    }
}
