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
            // Създаваме builder-а — той конфигурира всички услуги на приложението
            var builder = WebApplication.CreateBuilder(args);

            // Определяме пътя до папката Data и файла на SQLite базата данни
            var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Database");
            Directory.CreateDirectory(dataDir); // Създаваме папката ако не съществува
            var dbPath = Path.Combine(dataDir, "scooters.db");

            // Регистрираме MVC (контролери + Razor views)
            builder.Services.AddControllersWithViews();

            // Сесии — необходими за кошницата
            builder.Services.AddDistributedMemoryCache();
            builder.Services.AddHttpContextAccessor();
            builder.Services.AddSession(options =>
            {
                options.IdleTimeout = TimeSpan.FromMinutes(30);
                options.Cookie.HttpOnly = true;
                options.Cookie.IsEssential = true;
            });

            // Регистрираме ProductService — lifetime е Scoped (нов обект на всяка заявка)
            builder.Services.AddScoped<ProductService>();
            builder.Services.AddScoped<ImageUploadService>();
            builder.Services.AddScoped<CartService>();

            // Конфигурираме Entity Framework Core да използва SQLite база данни
            builder.Services.AddDbContext<AppDbContext>(options =>
                options.UseSqlite($"Data Source={dbPath}"));

            // Регистрираме PasswordHasher за хеширане на пароли на потребители
            builder.Services.AddScoped<IPasswordHasher<User>, PasswordHasher<User>>();

            // Конфигурираме автентикация с бисквитки (cookie-based authentication)
            builder.Services
                .AddAuthentication(CookieAuthenticationDefaults.AuthenticationScheme)
                .AddCookie(options =>
                {
                    options.LoginPath = "/Account/Login";           // Пренасочване при нужда от вход
                    options.LogoutPath = "/Account/Logout";         // Път за изход
                    options.AccessDeniedPath = "/Account/Login";    // При отказан достъп
                    options.ExpireTimeSpan = TimeSpan.FromDays(7);
                    options.SlidingExpiration = true;
                    options.Cookie.HttpOnly = true;
                    options.Cookie.Name = "VoltRide.Auth";
                });

            // Регистрираме авторизация (проверка на права)
            builder.Services.AddAuthorization();

            // Изграждаме приложението
            var app = builder.Build();

            // Изпълняваме миграции и seed данни при стартиране
            using (var scope = app.Services.CreateScope())
            {
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                // Прилагаме всички чакащи миграции (създаваме/обновяваме таблиците)
                db.Database.Migrate();

                // Зареждаме начални данни ако базата е празна
                DbSeeder.Seed(db);

                // Обновяваме снимките за продукти без ImageUrl (еднократна корекция)
                var imageMap = new Dictionary<string, string>
                {
                    { "Spark Pro 350",   "/images/Бърз скутер.jfif" },
                    { "Thunder Max 500", "/images/Скоростна тротинетка.jfif" },
                    { "Eco Fold 200",    "/images/Ел. Тротинетка.jfif" },
                    { "Volt City 250",   "/images/G2-1200x1200.jpg" }
                };

                // За всеки продукт без снимка, търсим подходяща снимка по името му
                foreach (var product in db.Products.Where(p => p.ImageUrl == null))
                {
                    if (imageMap.TryGetValue(product.Name, out var url))
                    {
                        product.ImageUrl = url;
                    }
                }
                db.SaveChanges();
            }

            // Конфигурираме middleware pipeline (редът е важен!)

            if (!app.Environment.IsDevelopment())
            {
                app.UseExceptionHandler("/Home/Error");
                app.UseHsts();
                app.UseHttpsRedirection();
            }

            // Разрешаваме достъп до статични файлове (CSS, JS, снимки от wwwroot)
            app.UseStaticFiles();

            // Активираме маршрутизирането
            app.UseRouting();

            // Активираме сесиите (трябва преди Authentication/Authorization)
            app.UseSession();

            // Проверяваме кой е потребителят (автентикация) — трябва да е преди Authorization
            app.UseAuthentication();

            // Проверяваме дали потребителят има право (авторизация)
            app.UseAuthorization();

            // Основен маршрут: /Контролер/Действие/Id
            // По подразбиране отива на Home/Index
            app.MapControllerRoute(
                name: "default",
                pattern: "{controller=Home}/{action=Index}/{id?}");

            // Допълнителен маршрут за RoutingController
            app.MapControllerRoute(
                name: "routing",
                pattern: "{controller=Routing}/{*default}",
                defaults: new { controller = "Routing", action = "Default" });

            // Стартираме приложението
            app.Run();
        }
    }
}
