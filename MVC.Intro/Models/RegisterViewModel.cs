using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class RegisterViewModel
    {
        [DisplayName("Име")]
        [Required(ErrorMessage = "Името е задължително")]
        [StringLength(100, MinimumLength = 2, ErrorMessage = "Името трябва да е между 2 и 100 символа")]
        public string FullName { get; set; } = string.Empty;

        [DisplayName("Имейл")]
        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Въведете валиден имейл")]
        public string Email { get; set; } = string.Empty;

        [DisplayName("Парола")]
        [Required(ErrorMessage = "Паролата е задължителна")]
        [StringLength(100, MinimumLength = 6, ErrorMessage = "Паролата трябва да е поне 6 символа")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DisplayName("Потвърди парола")]
        [Required(ErrorMessage = "Потвърждението е задължително")]
        [DataType(DataType.Password)]
        [Compare(nameof(Password), ErrorMessage = "Паролите не съвпадат")]
        public string ConfirmPassword { get; set; } = string.Empty;
    }
}
