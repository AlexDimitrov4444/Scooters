using Microsoft.AspNetCore.Identity;
using MVC.Intro.Models;

namespace MVC.Intro.Data
{
    public static class DbSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.Products.Any())
            {
                context.Products.AddRange(
                    new Product
                    {
                        Id = Guid.NewGuid(),
                        Name = "Volt City 250",
                        Price = 649.99m,
                        Description = "Лека градска тротинетка за ежедневно придвижване.",
                        RangeKm = 25,
                        MaxSpeedKmh = 25,
                        ImageUrl = "/images/G2-1200x1200.jpg"
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

            if (!context.Users.Any())
            {
                var demo = new User
                {
                    Id = Guid.NewGuid(),
                    FullName = "Демо потребител",
                    Email = "demo@volt.bg",
                    PasswordHash = string.Empty
                };
                var hasher = new PasswordHasher<User>();
                demo.PasswordHash = hasher.HashPassword(demo, "Demo123!");
                context.Users.Add(demo);
            }

            context.SaveChanges();
        }
    }
}
