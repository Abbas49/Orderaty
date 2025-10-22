﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    public class CartController : Controller
    {
        private readonly AppDbContext db;
        public CartController(AppDbContext db)
        {
            this.db = db;
        }
        public IActionResult Index()
        {
            var clientId = db.Users.FirstOrDefault(c => c.UserName == User.Identity.Name)?.Id;
            var items = db.CartItems
                .Include(i => i.Product).ThenInclude(p => p.Seller).ThenInclude(s => s.User)
                .Where(c => c.ClientId == clientId)
                .ToList();
            return View(items);
        }

        [HttpPost]
        public void Add(int id, int quantity)
        {
            var clientId = db.Users.FirstOrDefault(c => c.UserName == User.Identity.Name)?.Id;
            var sellerId = db.Products.Include(p => p.Seller).Where(p => p.Id == id).FirstOrDefault()?.SellerId;
            var isDifferent = db.CartItems.Include(i => i.Product)
                .Any(c => c.ClientId == clientId &&
                          c.Product.SellerId != sellerId);

            if (!isDifferent)
            {
                var item = db.CartItems
                    .FirstOrDefault(c => c.ClientId == clientId && c.ProductId == id);
                if(item != null)
                {
                    item.Quantity += quantity;
                    db.SaveChanges();
                }
                else
                {
                    item = new CartItem
                    {
                        ClientId = clientId,
                        ProductId = id,
                        Quantity = quantity
                    };
                    db.Add(item);
                    db.SaveChanges();
                }  
            }
            return;
        }

        [HttpPost]
        public IActionResult Remove(int id)
        {
            var item = db.CartItems.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                db.Remove(item);
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Update(int id, int quantity)
        {
            var item = db.CartItems.FirstOrDefault(c => c.Id == id);
            if (item != null)
            {
                item.Quantity = quantity;
                db.SaveChanges();
            }
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult Clear()
        {
            var clientId = db.Users.FirstOrDefault(c => c.UserName == User.Identity.Name)?.Id;
            var items = db.CartItems.Where(c => c.ClientId == clientId).ToList();
            db.RemoveRange(items);
            db.SaveChanges();
            return RedirectToAction("Index");
        }
    }
}
