using System.ComponentModel.DataAnnotations;

namespace MVC.Intro.Models
{
    public class User
    {
        public Guid Id { get; set; }

        [MaxLength(100)]
        public required string FullName { get; set; }

        [MaxLength(200)]
        public required string Email { get; set; }

        public required string PasswordHash { get; set; }
    }
}
