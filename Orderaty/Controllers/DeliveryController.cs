using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;

namespace Orderaty.Controllers
{
    public class DeliveryController : Controller
    {
        private readonly AppDbContext db;
        private readonly UserManager<User> userManager;
        private readonly IWebHostEnvironment hostingEnvironment;

        public DeliveryController(AppDbContext db, UserManager<User> userManager, IWebHostEnvironment hostingEnvironment)
        {
            this.db = db;
            this.userManager = userManager;
            this.hostingEnvironment = hostingEnvironment;
        }

        // ✅ صفحة Home
        public IActionResult Home()
        {
            return View();
        }

        // ✅ Dashboard with Statistics
        public async Task<IActionResult> Dashboard()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var delivery = await db.Deliveries
                .Include(d => d.User)
                .Include(d => d.Orders)
                    .ThenInclude(o => o.OrderedItems)
                .Include(d => d.Orders)
                    .ThenInclude(o => o.Client)
                        .ThenInclude(c => c.User)
                .Include(d => d.Orders)
                    .ThenInclude(o => o.Seller)
                        .ThenInclude(s => s.User)
                .FirstOrDefaultAsync(d => d.Id == user.Id);

            if (delivery == null)
                return NotFound();

            var now = DateTime.Now;
            var startOfToday = now.Date;
            var startOfWeek = now.Date.AddDays(-(int)now.DayOfWeek);
            var startOfMonth = new DateTime(now.Year, now.Month, 1);

            // Calculate statistics
            var allDeliveries = delivery.Orders.Where(o => o.Status == OrderStatus.Delivered).ToList();
            
            // Total deliveries
            ViewBag.TotalDeliveriesToday = allDeliveries.Count(o => o.CreatedAt >= startOfToday);
            ViewBag.TotalDeliveriesWeek = allDeliveries.Count(o => o.CreatedAt >= startOfWeek);
            ViewBag.TotalDeliveriesMonth = allDeliveries.Count(o => o.CreatedAt >= startOfMonth);
            ViewBag.TotalDeliveriesAll = allDeliveries.Count;

            // Earnings (assuming delivery fee is 10% of order total)
            decimal deliveryFeePercentage = 0.10m;
            ViewBag.EarningsToday = allDeliveries.Where(o => o.CreatedAt >= startOfToday).Sum(o => o.TotalPrice * deliveryFeePercentage);
            ViewBag.EarningsWeek = allDeliveries.Where(o => o.CreatedAt >= startOfWeek).Sum(o => o.TotalPrice * deliveryFeePercentage);
            ViewBag.EarningsMonth = allDeliveries.Where(o => o.CreatedAt >= startOfMonth).Sum(o => o.TotalPrice * deliveryFeePercentage);
            ViewBag.EarningsAll = allDeliveries.Sum(o => o.TotalPrice * deliveryFeePercentage);

            // Active deliveries (Processing + Shipped)
            ViewBag.ActiveDeliveries = delivery.Orders.Count(o => o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped);

            // Average delivery time (mock calculation - hours between order creation and delivery)
            var deliveredOrders = allDeliveries.Where(o => o.CreatedAt.Date >= startOfMonth).ToList();
            if (deliveredOrders.Any())
            {
                // Assuming average delivery takes 2-4 hours (mock data)
                ViewBag.AverageDeliveryTime = 3.2; // in hours
            }
            else
            {
                ViewBag.AverageDeliveryTime = 0;
            }

            // Performance rating (mock - based on delivery count and completion)
            var completionRate = delivery.Orders.Any() ? 
                (decimal)allDeliveries.Count / delivery.Orders.Count * 100 : 0;
            ViewBag.PerformanceRating = Math.Min(5.0m, completionRate / 20); // Convert to 5-star scale

            // Recent activity - last 10 orders
            ViewBag.RecentOrders = delivery.Orders
                .OrderByDescending(o => o.CreatedAt)
                .Take(10)
                .ToList();

            // Orders by status
            ViewBag.PendingDeliveries = db.Orders.Count(o => o.Status == OrderStatus.PendingDelivery);
            ViewBag.ProcessingOrders = delivery.Orders.Count(o => o.Status == OrderStatus.Processing);
            ViewBag.ShippedOrders = delivery.Orders.Count(o => o.Status == OrderStatus.Shipped);

            return View(delivery);
        }

        // ✅ عرض البروفايل
        public async Task<IActionResult> Profile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var delivery = await db.Deliveries
                .Include(d => d.User)
                .FirstOrDefaultAsync(d => d.Id == user.Id);

            if (delivery == null)
                return NotFound();

            return View(delivery);
        }

        // ✅ تعديل البيانات
        [HttpGet]
        public async Task<IActionResult> EditProfile()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var model = new EditDeliveryVM
            {
                FullName = user.FullName,
                PhoneNumber = user.PhoneNumber,
                CurrentImage = user.Image
            };

            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> EditProfile(EditDeliveryVM model)
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            if (!ModelState.IsValid)
                return View(model);

            user.FullName = model.FullName;
            user.PhoneNumber = model.PhoneNumber;

            if (model.NewImage != null)
            {
                user.Image = await SaveImage(model.NewImage);
            }

            db.Users.Update(user);
            await db.SaveChangesAsync();

            return RedirectToAction("Profile");
        }

        // ✅ دالة حفظ الصورة
        private async Task<string> SaveImage(IFormFile imageFile)
        {
            var folderPath = Path.Combine(hostingEnvironment.WebRootPath, "images", "users");
            if (!Directory.Exists(folderPath))
                Directory.CreateDirectory(folderPath);

            var fileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
            var filePath = Path.Combine(folderPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await imageFile.CopyToAsync(stream);
            }

            return fileName;
        }

        // ✅ عرض الطلبات اللي مستنية دليفري
        public async Task<IActionResult> Orders()
        {
            var pendingOrders = await db.Orders
                .Include(o => o.Seller).ThenInclude(s => s.User)
                .Include(o => o.Client).ThenInclude(c => c.User)
                .Where(o => o.Status == OrderStatus.PendingDelivery)
                .ToListAsync();

            return View(pendingOrders);
        }

        // ✅ عرض تفاصيل الطلب
        public async Task<IActionResult> OrderDetails(int id)
        {
            var order = await db.Orders
                .Include(o => o.Seller).ThenInclude(s => s.User)
                .Include(o => o.Client).ThenInclude(c => c.User)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
                return NotFound();

            return View(order);
        }

        // ✅ تحديث الحالة (Step by Step)
        [HttpPost]
        public async Task<IActionResult> UpdateStatus(int id)
        {
            var user = await userManager.GetUserAsync(User);
            var order = await db.Orders.FindAsync(id);
            if (order == null)
                return NotFound();

            switch (order.Status)
            {
                case OrderStatus.PendingDelivery:
                    order.Status = OrderStatus.Processing;
                    order.DeliveryId = user.Id; // ✅ يسجل الدليفري الحالي
                    break;
                case OrderStatus.Processing:
                    order.Status = OrderStatus.Shipped;
                    break;
                case OrderStatus.Shipped:
                    order.Status = OrderStatus.Delivered;
                    break;
            }

            await db.SaveChangesAsync();
            return RedirectToAction("OrderDetails", new { id });
        }



        [HttpGet]
        public async Task<IActionResult> MyDeliveries()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var myOrders = await db.Orders
                .Include(o => o.Seller).ThenInclude(s => s.User)
                .Include(o => o.Client).ThenInclude(c => c.User)
                .Where(o =>
                    o.DeliveryId == user.Id ||
                    (o.Status == OrderStatus.Processing || o.Status == OrderStatus.Shipped))
                .ToListAsync();

            return View(myOrders);
        }


        [HttpGet]
        public async Task<IActionResult> History()
        {
            var user = await userManager.GetUserAsync(User);
            if (user == null)
                return RedirectToAction("Login", "User");

            var completedOrders = await db.Orders
                .Include(o => o.Seller).ThenInclude(s => s.User)
                .Include(o => o.Client).ThenInclude(c => c.User)
                .Where(o => o.DeliveryId == user.Id && o.Status == OrderStatus.Delivered)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            return View(completedOrders);
        }

    }


}

