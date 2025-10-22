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


    }
}
