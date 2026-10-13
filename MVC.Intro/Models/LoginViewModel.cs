using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class LoginViewModel
    {
        [DisplayName("Имейл")]
        [Required(ErrorMessage = "Имейлът е задължителен")]
        [EmailAddress(ErrorMessage = "Въведете валиден имейл")]
        public string Email { get; set; } = string.Empty;

        [DisplayName("Парола")]
        [Required(ErrorMessage = "Паролата е задължителна")]
        [DataType(DataType.Password)]
        public string Password { get; set; } = string.Empty;

        [DisplayName("Запомни ме")]
        public bool RememberMe { get; set; }
    }
}
