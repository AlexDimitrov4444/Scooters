using Microsoft.AspNetCore.Identity;
using Scooters.Models;

namespace Scooters.Data
{
    // DbSeeder се използва за първоначално зареждане на данни в базата
    // Изпълнява се само веднъж — когато таблиците са празни
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            // Проверяваме дали вече има продукти — ако има, не добавяме нови
            if (!context.Products.Any())
            {
                // Добавяме примерни тротинетки с начални данни
                context.Products.AddRange(
                    new Product
                    {
                        Id = Guid.NewGuid(),           // Генерираме уникален ID
                        Name = "Volt City 250",
                        Price = 649.99m,
                        Description = "Лека градска тротинетка за ежедневно придвижване.",
                        RangeKm = 25,
                        MaxSpeedKmh = 25,
                        ImageUrl = "/images/G2-1200x1200.jpg"  // Снимка от wwwroot/images
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Spark Pro 350",
                        Price = 899.99m,
                        Description = "По-мощен мотор и по-дълъг пробег за пътуване до работа.",
                        RangeKm = 40,
                        MaxSpeedKmh = 32,
                        ImageUrl = "/images/Бърз скутер.jfif"
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Thunder Max 500",
                        Price = 1299.99m,
                        Description = "Двойно окачване и здрава рамка за по-дълги маршрути.",
                        RangeKm = 55,
                        MaxSpeedKmh = 40,
                        ImageUrl = "/images/Скоростна тротинетка.jfif"
                    },
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Eco Fold 200",
                        Price = 499.99m,
                        Description = "Сгъваем модел, удобен за обществен транспорт.",
                        RangeKm = 18,
                        MaxSpeedKmh = 20,
                        ImageUrl = "/images/Ел. Тротинетка.jfif"
                    });
            }

            // Проверяваме дали вече има потребители — ако има, не добавяме нови
            if (!context.Users.Any())
            {
                var hasher = new PasswordHasher<User>();

                // Създаваме демо потребител за тестване
                var demo = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Демо потребител",
                    Email = "demo@volt.bg",
                    PasswordHash = string.Empty  // Временно празно — ще се хешира по-долу
                };
                demo.IsAdmin = false;

                // Хешираме паролата преди запис (никога не пазим пароли в чист текст)
                demo.PasswordHash = hasher.HashPassword(demo, "Demo123!");

                context.Users.Add(demo);
            }

            // Добавяме администраторски акаунт ако не съществува
            if (!context.Users.Any(u => u.Email == "admin@scooters.bg"))
            {
                var hasher = new PasswordHasher<User>();
                var admin = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Администратор",
                    Email = "admin@scooters.bg",
                    PasswordHash = string.Empty,
                    IsAdmin = true
                };
                admin.PasswordHash = hasher.HashPassword(admin, "Admin123!");
                context.Users.Add(admin);
            }

            // Записваме всички промени в базата данни
            context.SaveChanges();
        }
    }
}
