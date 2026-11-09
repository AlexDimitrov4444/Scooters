using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace Scooters.Models
{
    // Един артикул в поръчката
    public class OrderItem
    {
        [Key]
        public Guid Id { get; set; } = Guid.NewGuid();

        // Поръчка, към която принадлежи артикулът
        public Guid OrderId { get; set; }
        public Order Order { get; set; } = null!;

        // Продукт
        public Guid ProductId { get; set; }

        [Required]
        [MaxLength(100)]
        public string ProductName { get; set; } = string.Empty;

        // Единична цена към момента на поръчката
        public decimal UnitPrice { get; set; }

        // Количество
        public int Quantity { get; set; }

        // Изчислено поле — не се записва в базата данни
        [NotMapped]
        public decimal Subtotal => UnitPrice * Quantity;
    }
}
