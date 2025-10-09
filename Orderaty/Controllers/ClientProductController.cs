using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    public class ClientProductController : Controller
    {
        private readonly AppDbContext db;

        public ClientProductController(AppDbContext db)
        {
            this.db = db;
        }

        // 🧾 عرض كل المنتجات مع البحث والفلترة
        public async Task<IActionResult> Index(string? search, string? category, string? sellerName,
                                       decimal? minPrice, decimal? maxPrice, string? sort)
        {
            var products = db.Products
                .Include(p => p.Seller).ThenInclude(s => s.User)
                .AsQueryable();

            // البحث بالاسم
            if (!string.IsNullOrEmpty(search))
                products = products.Where(p => p.Name.Contains(search));

            // فلترة بالفئة (Category)
            if (!string.IsNullOrEmpty(category))
            {
                if (Enum.TryParse<SellerCategory>(category, out var categoryEnum))
                {
                    products = products.Where(p => p.Seller.Category == categoryEnum);
                }
            }

            // فلترة بالبائع
            if (!string.IsNullOrEmpty(sellerName))
                products = products.Where(p => p.Seller.User.FullName.Contains(sellerName));

            // فلترة بالسعر
            if (minPrice.HasValue)
                products = products.Where(p => p.Price >= minPrice);
            if (maxPrice.HasValue)
                products = products.Where(p => p.Price <= maxPrice);

            // الترتيب
            products = sort switch
            {
                "rating_desc" => products.OrderByDescending(p => p.Rating),
                "rating_asc" => products.OrderBy(p => p.Rating),
                "price_asc" => products.OrderBy(p => p.Price),
                "price_desc" => products.OrderByDescending(p => p.Price),
                _ => products.OrderByDescending(p => p.Id)
            };

            var result = await products.ToListAsync();
            return View(result);
        }


        // 🔍 تفاصيل المنتج
        public async Task<IActionResult> Details(int id)
        {
            var product = await db.Products
                .Include(p => p.Seller).ThenInclude(s => s.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            return View(product);
        }
    }
}
