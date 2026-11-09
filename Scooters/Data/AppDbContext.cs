using Microsoft.EntityFrameworkCore;
using Scooters.Models;

namespace Scooters.Data
{
    // AppDbContext е "мостът" между C# обектите и базата данни (SQLite)
    // Наследява DbContext от Entity Framework Core
    public class AppDbContext : DbContext
    {
        // Конструктор — приема настройки (connection string, provider и т.н.) от dependency injection
        public AppDbContext(DbContextOptions<AppDbContext> options) : base(options)
        {
        }

        // Таблица с продукти (тротинетки) в базата данни
        public DbSet<Product> Products { get; set; } = null!;

        // Таблица с потребители в базата данни
        public DbSet<User> Users { get; set; } = null!;

        // Таблица с поръчки
        public DbSet<Order> Orders { get; set; } = null!;

        // Таблица с артикули от поръчките
        public DbSet<OrderItem> OrderItems { get; set; } = null!;

        // Метод за допълнителна конфигурация на модела при създаване на базата
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            // Задаваме уникален индекс на полето Email в таблицата Users
            // Това гарантира, че двама потребители не могат да имат един и същ имейл
            modelBuilder.Entity<User>()
                .HasIndex(u => u.Email)
                .IsUnique();

            // Поръчка → Потребител: изтриването на потребител не трие поръчките му
            modelBuilder.Entity<Order>()
                .HasOne(o => o.User)
                .WithMany()
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            // Поръчка → Артикули: каскадно изтриване (артикулите се изтриват с поръчката)
            modelBuilder.Entity<OrderItem>()
                .HasOne(oi => oi.Order)
                .WithMany(o => o.Items)
                .HasForeignKey(oi => oi.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
