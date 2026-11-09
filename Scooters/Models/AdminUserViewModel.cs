namespace Scooters.Models
{
    public class AdminUserViewModel
    {
        public Guid Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public bool IsAdmin { get; set; }
        public int OrderCount { get; set; }
    }
}
