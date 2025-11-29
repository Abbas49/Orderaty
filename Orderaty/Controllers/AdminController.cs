using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly RoleManager<IdentityRole> roleManager;

        public AdminController(AppDbContext db, UserManager<User> userManager, RoleManager<IdentityRole> roleManager)
        {
            this.db = db;
            this.userManager = userManager;
            this.roleManager = roleManager;
        }

        // ===================== DASHBOARD =====================
        public async Task<IActionResult> Dashboard()
        {
            // Platform Overview Statistics
            var totalUsers = await userManager.Users.CountAsync();
            var totalSellers = await db.Sellers.CountAsync();
            var totalOrders = await db.Orders.CountAsync();
            var totalRevenue = await db.Orders.SumAsync(o => (decimal?)o.TotalPrice) ?? 0;

            ViewBag.TotalUsers = totalUsers;
            ViewBag.TotalSellers = totalSellers;
            ViewBag.TotalOrders = totalOrders;
            ViewBag.TotalRevenue = totalRevenue;

            // Active Coupons
            var activeCoupons = await db.Coupons
                .Where(c => c.IsActive && c.ExpireDate >= DateTime.Now)
                .OrderByDescending(c => c.Id)
                .Take(5)
                .ToListAsync();

            ViewBag.ActiveCoupons = activeCoupons;
            ViewBag.ActiveCouponsCount = activeCoupons.Count;

            return View();
        }

        // ===================== USER MANAGEMENT =====================
        public async Task<IActionResult> Users(string search, string role, string status)
        {
            // Fetch all users
            var users = await userManager.Users.ToListAsync();

            // Store filter values in ViewBag for maintaining state
            ViewBag.SearchTerm = search;
            ViewBag.RoleFilter = role;
            ViewBag.StatusFilter = status;

            // Apply search filter
            if (!string.IsNullOrEmpty(search))
            {
                search = search.ToLower();
                users = users.Where(u => 
                    u.FullName.ToLower().Contains(search) ||
                    u.Email.ToLower().Contains(search) ||
                    u.UserName.ToLower().Contains(search)
                ).ToList();
            }

            // Apply role filter
            if (!string.IsNullOrEmpty(role))
            {
                var usersInRole = await userManager.GetUsersInRoleAsync(role);
                var userIdsInRole = usersInRole.Select(u => u.Id).ToHashSet();
                users = users.Where(u => userIdsInRole.Contains(u.Id)).ToList();
            }

            // Apply status filter
            if (!string.IsNullOrEmpty(status))
            {
                if (status == "Active")
                {
                    users = users.Where(u => !u.LockoutEnd.HasValue || u.LockoutEnd.Value <= DateTimeOffset.Now).ToList();
                }
                else if (status == "Suspended")
                {
                    users = users.Where(u => u.LockoutEnd.HasValue && u.LockoutEnd.Value > DateTimeOffset.Now).ToList();
                }
            }

            // Calculate role statistics (always based on all users, not filtered)
            var allUsers = await userManager.Users.ToListAsync();
            var clientUsers = await userManager.GetUsersInRoleAsync("Client");
            var sellerUsers = await userManager.GetUsersInRoleAsync("Seller");
            var deliveryUsers = await userManager.GetUsersInRoleAsync("Delivery");
            var adminUsers = await userManager.GetUsersInRoleAsync("Admin");

            // Set statistics in ViewBag
            ViewBag.TotalUsers = allUsers.Count;
            ViewBag.TotalClients = clientUsers.Count;
            ViewBag.TotalSellers = sellerUsers.Count;
            ViewBag.TotalDelivery = deliveryUsers.Count;

            // Create a dictionary of user roles for the view
            var userRoles = new Dictionary<string, List<string>>();
            foreach (var user in users)
            {
                var roles = await userManager.GetRolesAsync(user);
                userRoles[user.Id] = roles.ToList();
            }
            ViewBag.UserRoles = userRoles;

            return View(users);
        }

        public IActionResult UserDetails(string id)
        {
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> ToggleSuspendUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is an admin (cannot suspend admins)
            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return BadRequest("Cannot suspend admin users");
            }

            // Check if user is currently locked out
            var isCurrentlyLocked = user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.Now;

            if (isCurrentlyLocked)
            {
                // Unsuspend user - remove lockout
                user.LockoutEnd = null;
                await userManager.UpdateAsync(user);
            }
            else
            {
                // Suspend user - set lockout to far future
                user.LockoutEnd = DateTimeOffset.Now.AddYears(100);
                await userManager.UpdateAsync(user);
            }

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await userManager.FindByIdAsync(id);
            if (user == null)
            {
                return NotFound();
            }

            // Check if user is an admin (cannot delete admins)
            var isAdmin = await userManager.IsInRoleAsync(user, "Admin");
            if (isAdmin)
            {
                return BadRequest("Cannot delete admin users");
            }

            // Delete related records based on role
            var client = await db.Clients.FirstOrDefaultAsync(c => c.Id == id);
            if (client != null)
            {
                // Delete client's cart items
                var cartItems = await db.CartItems.Where(ci => ci.ClientId == client.Id).ToListAsync();
                db.CartItems.RemoveRange(cartItems);

                // Delete client's favourites
                var favourites = await db.Favourites.Where(f => f.ClientId == client.Id).ToListAsync();
                db.Favourites.RemoveRange(favourites);

                // Delete client's orders
                var orders = await db.Orders.Where(o => o.ClientId == client.Id).ToListAsync();
                foreach (var order in orders)
                {
                    // Delete ordered items
                    var orderedItems = await db.OrderedItems.Where(oi => oi.OrderId == order.Id).ToListAsync();
                    db.OrderedItems.RemoveRange(orderedItems);
                }
                db.Orders.RemoveRange(orders);

                // Delete client's reviews
                var productReviews = await db.ProductReviews.Where(pr => pr.ClientId == client.Id).ToListAsync();
                db.ProductReviews.RemoveRange(productReviews);

                var sellerReviews = await db.SellerReviews.Where(sr => sr.ClientId == client.Id).ToListAsync();
                db.SellerReviews.RemoveRange(sellerReviews);

                // Delete client
                db.Clients.Remove(client);
            }

            var seller = await db.Sellers.FirstOrDefaultAsync(s => s.Id == id);
            if (seller != null)
            {
                // Delete seller's products
                var products = await db.Products.Where(p => p.SellerId == seller.Id).ToListAsync();
                foreach (var product in products)
                {
                    // Delete product reviews
                    var productReviews = await db.ProductReviews.Where(pr => pr.ProductId == product.Id).ToListAsync();
                    db.ProductReviews.RemoveRange(productReviews);

                    // Delete cart items with this product
                    var cartItems = await db.CartItems.Where(ci => ci.ProductId == product.Id).ToListAsync();
                    db.CartItems.RemoveRange(cartItems);

                    // Delete ordered items with this product
                    var orderedItems = await db.OrderedItems.Where(oi => oi.ProductId == product.Id).ToListAsync();
                    db.OrderedItems.RemoveRange(orderedItems);
                }
                db.Products.RemoveRange(products);

                // Delete seller reviews
                var sellerReviews = await db.SellerReviews.Where(sr => sr.SellerId == seller.Id).ToListAsync();
                db.SellerReviews.RemoveRange(sellerReviews);

                // Delete favourites for this seller
                var favourites = await db.Favourites.Where(f => f.SellerId == seller.Id).ToListAsync();
                db.Favourites.RemoveRange(favourites);

                // Delete seller
                db.Sellers.Remove(seller);
            }

            var delivery = await db.Deliveries.FirstOrDefaultAsync(d => d.Id == id);
            if (delivery != null)
            {
                // Update orders to remove delivery reference
                var orders = await db.Orders.Where(o => o.DeliveryId == delivery.Id).ToListAsync();
                foreach (var order in orders)
                {
                    order.DeliveryId = null;
                }

                // Delete delivery
                db.Deliveries.Remove(delivery);
            }

            // Delete messages sent or received by the user
            var messages = await db.Messages.Where(m => m.SenderId == id || m.ReceiverId == id).ToListAsync();
            db.Messages.RemoveRange(messages);

            // Save changes to database
            await db.SaveChangesAsync();

            // Delete the user from Identity
            var result = await userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                return BadRequest("Failed to delete user");
            }

            return Ok();
        }

        // ===================== SELLER APPROVAL =====================
        public IActionResult SellerApproval()
        {
            return View();
        }

        public IActionResult SellerDetails(string id)
        {
            return View();
        }

        // ===================== COUPON MANAGEMENT =====================
        public async Task<IActionResult> Coupons()
        {
            // Fetch all coupons
            var coupons = await db.Coupons.Include(c => c.Orders).ToListAsync();

            // Calculate statistics
            var now = DateTime.Now;
            var activeCoupons = coupons.Where(c => c.IsActive && c.ExpireDate >= now).Count();
            var expiredCoupons = coupons.Where(c => c.ExpireDate < now).Count();
            var totalUsage = coupons.Sum(c => c.Orders?.Count ?? 0);

            ViewBag.ActiveCount = activeCoupons;
            ViewBag.ExpiredCount = expiredCoupons;
            ViewBag.TotalUsage = totalUsage;
            ViewBag.TotalCount = coupons.Count;

            // Create usage count dictionary
            var couponUsage = new Dictionary<int, int>();
            foreach (var coupon in coupons)
            {
                couponUsage[coupon.Id] = coupon.Orders?.Count ?? 0;
            }
            ViewBag.CouponUsage = couponUsage;

            return View(coupons);
        }

        [HttpPost]
        public async Task<IActionResult> ToggleCoupon(int id)
        {
            var coupon = await db.Coupons.FindAsync(id);
            if (coupon == null)
            {
                return NotFound();
            }

            // Check if expired
            if (coupon.ExpireDate < DateTime.Now)
            {
                return BadRequest("Cannot toggle expired coupon");
            }

            // Toggle active status
            coupon.IsActive = !coupon.IsActive;
            await db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> DeleteCoupon(int id)
        {
            var coupon = await db.Coupons.Include(c => c.Orders).FirstOrDefaultAsync(c => c.Id == id);
            if (coupon == null)
            {
                return NotFound();
            }

            // Check if coupon has been used
            if (coupon.Orders != null && coupon.Orders.Any())
            {
                return BadRequest("Cannot delete coupon that has been used in orders");
            }

            db.Coupons.Remove(coupon);
            await db.SaveChangesAsync();

            return Ok();
        }

        public IActionResult CreateCoupon()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CreateCoupon(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                // Validate coupon code uniqueness
                var existingCoupon = await db.Coupons.FirstOrDefaultAsync(c => c.Code == coupon.Code);
                if (existingCoupon != null)
                {
                    ModelState.AddModelError("Code", "This coupon code already exists. Please use a different code.");
                    return View(coupon);
                }

                // Validate expiration date
                if (coupon.ExpireDate < DateTime.Now.Date)
                {
                    ModelState.AddModelError("ExpireDate", "Expiration date must be in the future.");
                    return View(coupon);
                }

                // Validate discount value
                if (coupon.DiscountValue <= 0)
                {
                    ModelState.AddModelError("DiscountValue", "Discount value must be greater than zero.");
                    return View(coupon);
                }

                // Validate minimum total
                if (coupon.MinimumTotal < 0)
                {
                    ModelState.AddModelError("MinimumTotal", "Minimum total cannot be negative.");
                    return View(coupon);
                }

                // Add coupon to database
                db.Coupons.Add(coupon);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' created successfully!";
                return RedirectToAction(nameof(Coupons));
            }

            return View(coupon);
        }

        public async Task<IActionResult> EditCoupon(int id)
        {
            var coupon = await db.Coupons
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == id);
            
            if (coupon == null)
            {
                return NotFound();
            }

            // Calculate usage statistics
            ViewBag.TimesUsed = coupon.Orders?.Count ?? 0;
            ViewBag.TotalSavings = coupon.Orders?.Sum(o => coupon.DiscountValue) ?? 0;
            ViewBag.LastUsed = coupon.Orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("MMM dd, yyyy");

            return View(coupon);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCoupon(Coupon coupon)
        {
            if (ModelState.IsValid)
            {
                // Check if code already exists (excluding current coupon)
                var existingCoupon = await db.Coupons
                    .FirstOrDefaultAsync(c => c.Code == coupon.Code && c.Id != coupon.Id);
                
                if (existingCoupon != null)
                {
                    ModelState.AddModelError("Code", "This coupon code already exists.");
                    
                    // Reload usage statistics for view
                    var couponWithOrders = await db.Coupons
                        .Include(c => c.Orders)
                        .FirstOrDefaultAsync(c => c.Id == coupon.Id);
                    ViewBag.TimesUsed = couponWithOrders?.Orders?.Count ?? 0;
                    ViewBag.TotalSavings = couponWithOrders?.Orders?.Sum(o => coupon.DiscountValue) ?? 0;
                    ViewBag.LastUsed = couponWithOrders?.Orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("MMM dd, yyyy");
                    
                    return View(coupon);
                }

                // Validate expiration date
                if (coupon.ExpireDate < DateTime.Now.Date)
                {
                    ModelState.AddModelError("ExpireDate", "Expiration date must be in the future.");
                    
                    // Reload usage statistics for view
                    var couponWithOrders = await db.Coupons
                        .Include(c => c.Orders)
                        .FirstOrDefaultAsync(c => c.Id == coupon.Id);
                    ViewBag.TimesUsed = couponWithOrders?.Orders?.Count ?? 0;
                    ViewBag.TotalSavings = couponWithOrders?.Orders?.Sum(o => coupon.DiscountValue) ?? 0;
                    ViewBag.LastUsed = couponWithOrders?.Orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("MMM dd, yyyy");
                    
                    return View(coupon);
                }

                // Validate discount value
                if (coupon.DiscountValue <= 0)
                {
                    ModelState.AddModelError("DiscountValue", "Discount value must be greater than zero.");
                    
                    // Reload usage statistics for view
                    var couponWithOrders = await db.Coupons
                        .Include(c => c.Orders)
                        .FirstOrDefaultAsync(c => c.Id == coupon.Id);
                    ViewBag.TimesUsed = couponWithOrders?.Orders?.Count ?? 0;
                    ViewBag.TotalSavings = couponWithOrders?.Orders?.Sum(o => coupon.DiscountValue) ?? 0;
                    ViewBag.LastUsed = couponWithOrders?.Orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("MMM dd, yyyy");
                    
                    return View(coupon);
                }

                // Validate minimum total
                if (coupon.MinimumTotal < 0)
                {
                    ModelState.AddModelError("MinimumTotal", "Minimum total cannot be negative.");
                    
                    // Reload usage statistics for view
                    var couponWithOrders = await db.Coupons
                        .Include(c => c.Orders)
                        .FirstOrDefaultAsync(c => c.Id == coupon.Id);
                    ViewBag.TimesUsed = couponWithOrders?.Orders?.Count ?? 0;
                    ViewBag.TotalSavings = couponWithOrders?.Orders?.Sum(o => coupon.DiscountValue) ?? 0;
                    ViewBag.LastUsed = couponWithOrders?.Orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("MMM dd, yyyy");
                    
                    return View(coupon);
                }

                // Update coupon in database
                db.Coupons.Update(coupon);
                await db.SaveChangesAsync();

                TempData["SuccessMessage"] = $"Coupon '{coupon.Code}' updated successfully!";
                return RedirectToAction(nameof(Coupons));
            }

            // Reload usage statistics for view if ModelState is invalid
            var loadedCoupon = await db.Coupons
                .Include(c => c.Orders)
                .FirstOrDefaultAsync(c => c.Id == coupon.Id);
            ViewBag.TimesUsed = loadedCoupon?.Orders?.Count ?? 0;
            ViewBag.TotalSavings = loadedCoupon?.Orders?.Sum(o => coupon.DiscountValue) ?? 0;
            ViewBag.LastUsed = loadedCoupon?.Orders?.OrderByDescending(o => o.CreatedAt).FirstOrDefault()?.CreatedAt.ToString("MMM dd, yyyy");

            return View(coupon);
        }


    }
}
