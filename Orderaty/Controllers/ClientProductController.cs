using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;

namespace Orderaty.Controllers
{
    [Authorize(Roles = "Client")]
    public class ClientProductController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;

        public ClientProductController(AppDbContext db, UserManager<User> userManager)
        {
            this.db = db;
            this.userManager = userManager;
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
                .Include(p => p.ProductReviews)
                    .ThenInclude(pr => pr.Client)
                        .ThenInclude(c => c.User)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Check if current user has already reviewed this product
            if (User.Identity.IsAuthenticated)
            {
                var user = await userManager.GetUserAsync(User);
                if (user != null)
                {
                    var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == user.Id);
                    if (client != null)
                    {
                        var existingReview = await db.ProductReviews
                            .FirstOrDefaultAsync(pr => pr.ProductId == id && pr.ClientId == client.Id);
                        ViewBag.HasReviewed = existingReview != null;
                        ViewBag.ExistingReview = existingReview;
                    }
                }
            }

            return View(product);
        }

        // --------------------- ⭐ Create Product Review ---------------------
        [HttpPost]
        public async Task<IActionResult> CreateReview(ProductReviewVM model)
        {
            if (!ModelState.IsValid)
            {
                TempData["Error"] = "Please provide a valid rating.";
                return RedirectToAction("Details", new { id = model.ProductId });
            }

            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == user.Id);
            if (client == null)
                return RedirectToAction("Login", "User");

            // Check if product exists
            var product = await db.Products.FirstOrDefaultAsync(p => p.Id == model.ProductId);
            if (product == null)
                return NotFound();

            // Check if client has already reviewed this product
            var existingReview = await db.ProductReviews
                .FirstOrDefaultAsync(pr => pr.ProductId == model.ProductId && pr.ClientId == client.Id);

            decimal? oldRating = null;
            if (existingReview != null)
            {
                // Store old rating before updating
                oldRating = existingReview.Rating;
                // Update existing review
                existingReview.Rating = model.Rating;
                existingReview.Comment = model.Comment;
            }
            else
            {
                // Create new review
                var review = new ProductReview
                {
                    Rating = model.Rating,
                    Comment = model.Comment,
                    ClientId = client.Id,
                    ProductId = model.ProductId
                };
                await db.ProductReviews.AddAsync(review);
            }

            // Save changes first to ensure database has updated values
            await db.SaveChangesAsync();

            // Update product's average rating after saving changes
            var reviews = await db.ProductReviews
                .Where(pr => pr.ProductId == model.ProductId)
                .ToListAsync();

            if (reviews.Any())
            {
                product.Rating = reviews.Average(r => r.Rating);
            }
            else
            {
                // If no reviews exist (shouldn't happen, but handle edge case)
                product.Rating = 0;
            }

            await db.SaveChangesAsync();

            TempData["Success"] = existingReview != null ? "Review updated successfully!" : "Review submitted successfully!";
            return RedirectToAction("Details", new { id = model.ProductId });
        }
    }
}
