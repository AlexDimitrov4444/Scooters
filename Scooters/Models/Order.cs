using System.ComponentModel.DataAnnotations;

namespace Scooters.Models
{
    // Модел на поръчка
    public class Order
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Потребител, направил поръчката
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;

        // Дата и час на създаване на поръчката (UTC)
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Текущ статус на поръчката
        public OrderStatus Status { get; set; } = OrderStatus.Pending;

        // Данни за доставка
        [Required]
        [MaxLength(200)]
        public string FullName { get; set; } = string.Empty;

        [Required]
        [MaxLength(30)]
        public string Phone { get; set; } = string.Empty;

        [Required]
        [MaxLength(300)]
        public string Address { get; set; } = string.Empty;

        [Required]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        // Обща сума на поръчката
        public decimal Total { get; set; }

        // Артикули в поръчката
        public List<OrderItem> Items { get; set; } = new();
    }
}
