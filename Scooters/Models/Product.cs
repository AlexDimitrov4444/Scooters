using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Scooters.Models
{
    // Модел за продукт (тротинетка) — представлява един ред в таблицата Products в базата данни
    public class Product
    {
        // Уникален идентификатор на продукта (генерира се автоматично като GUID)
        [DisplayName("Идентификатор")]
        public Guid Id { get; set; }

        // Наименование/модел на тротинетката
        // Задължително поле — между 3 и 50 символа, само букви, цифри, интервали и тирета
        [DisplayName("Модел")]
        [Required(ErrorMessage = "Полето е задължително")]
        [StringLength(50, MinimumLength = 3, ErrorMessage = "Дължината на името трябва да е между 3 и 50 символа")]
        [RegularExpression(@"^[A-Za-zА-Яа-яЁё0-9\s\-]+$", ErrorMessage = "Името може да съдържа само букви, цифри, интервали и тирета")]
        public required string Name { get; set; }

        // Цена в лева — задължително поле, между 24.99 и 9999.99 лв.
        [DisplayName("Цена (eur.)")]
        [Required(ErrorMessage = "Задължително е продуктът да има цена")]
        [Range(24.99, 9999.99, ErrorMessage = "Цената трябва да е между 24.99 и 9999.99")]
        public decimal Price { get; set; }

        // Описание на продукта — незадължително, максимум 300 символа
        [DisplayName("Описание")]
        [StringLength(300)]
        public string? Description { get; set; }

        // Пробег на батерията в километри — между 1 и 200 км
        [DisplayName("Пробег (км)")]
        [Range(1, 200, ErrorMessage = "Пробегът трябва да е между 1 и 200 км")]
        public int RangeKm { get; set; }

        // Максимална скорост в км/ч — между 5 и 80 км/ч
        [DisplayName("Макс. скорост (км/ч)")]
        [Range(5, 80, ErrorMessage = "Скоростта трябва да е между 5 и 80 км/ч")]
        public int MaxSpeedKmh { get; set; }

        // Относителен URL към снимката на продукта (напр. "/images/scooter.jpg")
        // Незадължително — ако е null, се показва емоджи по подразбиране
        [DisplayName("Снимка")]
        public string? ImageUrl { get; set; }
    }
}
