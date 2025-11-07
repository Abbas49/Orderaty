using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty.Controllers
{
    [Authorize(Roles = "Client")]
    public class FavouriteController : Controller
    {
        private readonly AppDbContext db;
        public FavouriteController(AppDbContext db)
        {
            this.db = db;
        }
        public bool Make(string sellerId)
        {
            if(User.Identity != null && User.Identity.IsAuthenticated)
            {
                var clientId = db.Users.FirstOrDefault(u => u.UserName == User.Identity.Name)?.Id;
                var isSellerExist = db.Sellers.Any(f => f.Id == sellerId);
                if (clientId != null && isSellerExist)
                {
                    var data = db.Favourites.Where(f => f.ClientId == clientId && f.SellerId == sellerId)
                        .FirstOrDefault();
                    if (data != null) 
                    {
                        data.IsFavourite = !data.IsFavourite;
                        db.SaveChanges();
                        return data.IsFavourite;
                    }
                    var favourite = new Favourite()
                    {
                        ClientId = clientId,
                        SellerId = sellerId,
                        IsFavourite = true
                    };
                    db.Favourites.Add(favourite);
                    db.SaveChanges();
                    return true;
                }
                
            }
            return false;
        }
    }
}
