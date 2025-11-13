using System.Diagnostics;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;
using Orderaty.ViewModels;

namespace Orderaty.Controllers;

public class HomeController : Controller
{
    private readonly ILogger<HomeController> _logger;
    private readonly AppDbContext db;
    public HomeController(ILogger<HomeController> logger, AppDbContext db)
    {
        _logger = logger;
        this.db = db;
    }

    public IActionResult Index()
    {
        var products = db.Products.Include(p => p.Seller).ThenInclude(p => p.User).Take(3).ToList();
        var sellers = db.Sellers.Include(s => s.User).Include(f => f.Favourites).Take(3).ToList();
        
        var data = new StoresProducts()
        {
            Products = products,
            Sellers = sellers
        };
        return View(data);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
    }
}
