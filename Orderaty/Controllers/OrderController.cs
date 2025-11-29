using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    public class OrderController : Controller
    {
        private readonly AppDbContext db;
        public OrderController(AppDbContext db)
        {
            this.db = db;
        }

        public IActionResult History()
        {
           if (User.Identity != null && User.Identity.IsAuthenticated)
           {
               var clientId = db.Users.Where(c => c.UserName == User.Identity.Name)
                   .FirstOrDefault()?.Id;
               var orders = db.Orders.Include(o => o.OrderedItems).ThenInclude(oi => oi.Product)
                   .Include(o => o.Seller).ThenInclude(s => s.User)
                   .Include(o => o.Coupon)
                   .Where(c => c.ClientId == clientId)
                   .OrderByDescending(o => o.CreatedAt)
                   .ToList();

               if (orders != null)
                   return View(orders);

               return View(new List<Order>());
           }
           return RedirectToAction("Login", "User");
        }

        public IActionResult Details(int id)
        {
            var order = db.Orders.Include(o => o.Seller).ThenInclude(s => s.User)
                .Include(o => o.OrderedItems).ThenInclude(oi => oi.Product)
                .FirstOrDefault(o => o.Id == id);
            return View(order);
        }

        public IActionResult Checkout()
        {
            if (User.Identity != null && User.Identity.IsAuthenticated)
            {
                var clientId = db.Users.FirstOrDefault(c => c.UserName == User.Identity.Name)?.Id;
                var cartItems = db.CartItems.Include(t => t.Product).Where(ci => ci.ClientId == clientId).ToList();
                const decimal deliveryFee = 15.00m;
                var order = new Order
                {
                    ClientId = clientId,
                    CreatedAt = DateTime.Now,
                    Status = OrderStatus.PendingDelivery,
                    TotalPrice = cartItems.Sum(ci => ci.Product.Price * ci.Quantity) + deliveryFee,
                    SellerId = cartItems.FirstOrDefault()?.Product.SellerId,
                };
                db.Orders.Add(order);
                db.SaveChanges();
                List<OrderedItem> orderItems = new List<OrderedItem>();
                foreach (var item in cartItems)
                {
                    // Decrease product stock
                    var product = db.Products.Find(item.ProductId);
                    if (product != null)
                    {
                        product.Available_Amount -= item.Quantity;
                        if (product.Available_Amount < 0)
                        {
                            product.Available_Amount = 0; // Prevent negative stock
                        }
                    }

                    orderItems.Add(new OrderedItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        Quantity = item.Quantity,
                        Subtotal = item.Product.Price
                    });
                }
                db.OrderedItems.AddRange(orderItems);
                db.CartItems.RemoveRange(cartItems);
                db.SaveChanges();
            }
            return RedirectToAction("Index", "ClientProduct");
        }

        // public IActionResult History()
        // {
        //     if(User.Identity != null && User.Identity.IsAuthenticated)
        //     {
        //         var clientId = db.Users.Where(c => c.UserName == User.Identity.Name)
        //             .FirstOrDefault()?.Id;
        //         var orders = db.Orders.Include(o => o.OrderedItems)
        //             .Include(o => o.Seller).ThenInclude(o => o.User)
        //             .Where(c => c.ClientId == clientId).ToList();

        //         if(orders != null)
        //             return View(orders);

        //         return NoContent();
        //     }
        //     return NotFound();
        // }
    }
}
