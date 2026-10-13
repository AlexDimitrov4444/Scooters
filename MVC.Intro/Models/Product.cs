using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class Product
    {
        [DisplayName("Идентификатор")]
        public Guid Id { get; set; }

        [DisplayName("Модел")]
        [Required(ErrorMessage = "Полето е задължително")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Дължината на името трябва да е между 3 и 50 символа")]
        [RegularExpression(@"^[A-Za-z0-9\s\-]+$", ErrorMessage = "Името може да съдържа само букви, цифри, интервали и тирета")]
        public required string Name { get; set; }

        [DisplayName("Цена (лв.)")]
        [Required(ErrorMessage = "Задължително е продуктът да има цена")]
        [Range(24.99, 9999.99, ErrorMessage = "Цената трябва да е между 24.99 и 9999.99")]
        public decimal Price { get; set; }

        [DisplayName("Описание")]
        [StringLength(300)]
        public string? Description { get; set; }

        [DisplayName("Пробег (км)")]
        [Range(1, 200, ErrorMessage = "Пробегът трябва да е между 1 и 200 км")]
        public int RangeKm { get; set; }

        [DisplayName("Макс. скорост (км/ч)")]
        [Range(5, 80, ErrorMessage = "Скоростта трябва да е между 5 и 80 км/ч")]
        public int MaxSpeedKmh { get; set; }

        [DisplayName("Снимка")]
        public string? ImageUrl { get; set; }
    }
}
