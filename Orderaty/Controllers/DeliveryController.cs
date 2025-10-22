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

