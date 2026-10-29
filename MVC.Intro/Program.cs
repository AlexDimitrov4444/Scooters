using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var builder = WebApplication.CreateBuilder(args);

            var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");
            Directory.CreateDirectory(dataDir);
            var dbPath = Path.Combine(dataDir, "scooters.db");

            builder.Services.AddControllersWithViews();
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";
                    options.LogoutPath = "/Account/Logout";
                    options.AccessDeniedPath = "/Account/Login";
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                });
            builder.Services.AddAuthorization();

            var app = builder.Build();

            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                db.Database.Migrate();
                DbSeeder.Seed(db);

                // Обновяване на снимките за продукти без ImageUrl
                var imageMap = new Dictionary<string, string>
                {
                    { "Spark Pro 350",   "/images/Бърз скутер.jfif" },
                    { "Thunder Max 500", "/images/Скоростна тротинетка.jfif" },
                    { "Eco Fold 200",    "/images/Ел. Тротинетка.jfif" },
                    { "Volt City 250",   "/images/G2-1200x1200.jpg" }
                };
                foreach (var product in db.Products.Where(p => p.ImageUrl == null))
                {
                    if (imageMap.TryGetValue(product.Name, out var url))
                    {
                        product.ImageUrl = url;
                    }
                }
                db.SaveChanges();
            }

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
            }

            app.UseHttpsRedirection();
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthentication();
            app.UseAuthorization();

            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            app.MapControllerRoute(
                name: "routing",
                pattern: "{controller=Routing}/{*default}",
                defaults: new { controller = "Routing", action = "Default" });

            app.Run();
        }
    }
}
