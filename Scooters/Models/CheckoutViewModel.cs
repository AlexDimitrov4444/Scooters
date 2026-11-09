using System.ComponentModel.DataAnnotations;

namespace Scooters.Models
{
    // View model за страницата за плащане / завършване на поръчка
    public class CheckoutViewModel
    {
        [Required(ErrorMessage = "Полето е задължително.")]
        [Display(Name = "Три имена")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето е задължително.")]
        [Display(Name = "Телефон")]
        public string Phone { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето е задължително.")]
        [Display(Name = "Адрес")]
        public string Address { get; set; } = string.Empty;

        [Required(ErrorMessage = "Полето е задължително.")]
        [Display(Name = "Град")]
        public string City { get; set; } = string.Empty;

        // Артикули от кошницата
        public List<CartItem> Items { get; set; } = new();

        // Обща сума
        public decimal Total { get; set; }
    }
}
