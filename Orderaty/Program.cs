using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Orderaty.Data;
using Orderaty.Models;

namespace Orderaty
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            // Add services to the container.
            builder.Services.AddControllersWithViews();
            builder.Services.AddDbContext<AppDbContext>(options => 
            options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection")));
            builder.Services.AddIdentity<User, IdentityRole>(options =>
            {
                options.Password.RequireDigit = true;
                options.Password.RequiredLength = 8;
                options.Password.RequireNonAlphanumeric = false;
                //options.Lockout.MaxFailedAccessAttempts = 5;
                options.User.RequireUniqueEmail = true;
                //options.SignIn.RequireConfirmedEmail = true;
            })
                .AddEntityFrameworkStores<AppDbContext>()
                .AddDefaultTokenProviders();

            builder.Services.ConfigureApplicationCookie(options =>
            {
                options.LoginPath = "/User/Login";
                options.LogoutPath = "/User/Logout";
                options.ExpireTimeSpan = TimeSpan.FromMinutes(30);
                options.SlidingExpiration = true;
            });

            var app = builder.Build();
            SeedData(app.Services.CreateScope().ServiceProvider).Wait();

            // Configure the HTTP request pipeline.
            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");


            app.Run();
        }

        private static async Task SeedData(IServiceProvider serviceProvider)
        {
            var userManager = serviceProvider.GetRequiredService<UserManager<User>>();
            var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

            if (!await roleManager.RoleExistsAsync("Admin"))
            {
                await roleManager.CreateAsync(new IdentityRole("Admin"));
            }
            if (!await roleManager.RoleExistsAsync("Client"))
            {
                await roleManager.CreateAsync(new IdentityRole("Client"));
            }
            if (!await roleManager.RoleExistsAsync("Seller"))
            {
                await roleManager.CreateAsync(new IdentityRole("Seller"));
            }
            if (!await roleManager.RoleExistsAsync("Delivery"))
            {
                await roleManager.CreateAsync(new IdentityRole("Delivery"));
            }

            var existingUser = await userManager.FindByEmailAsync("admin@orderaty.com");
            if (existingUser == null)
            {
                var user = new User
                {
                    FullName = "System Administrator",
                    UserName = "Admin",
                    Email = "admin@orderaty.com",
                };

                await userManager.CreateAsync(user, "Admin@123");

                await userManager.AddToRoleAsync(user, "Admin");
            }
        }

    }
}
