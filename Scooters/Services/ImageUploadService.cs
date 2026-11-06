namespace Scooters.Services
{
    public class ImageUploadService
    {
        private static readonly HashSet<string> AllowedExtensions =
            new(StringComparer.OrdinalIgnoreCase) { ".jpg", ".jpeg", ".png", ".gif", ".webp", ".jfif" };

        private const long MaxBytes = 5 * 1024 * 1024;

        private readonly IWebHostEnvironment _environment;

        public ImageUploadService(IWebHostEnvironment environment)
        {
            _environment = environment;
        }

        public async Task<(string? Url, string? Error)> SaveAsync(IFormFile? imageFile)
        {
            if (imageFile == null || imageFile.Length == 0)
            {
                return (null, null);
            }

            if (imageFile.Length > MaxBytes)
            {
                return (null, "Снимката трябва да е до 5 MB.");
            }

            var extension = Path.GetExtension(imageFile.FileName);
            if (string.IsNullOrWhiteSpace(extension) || !AllowedExtensions.Contains(extension))
            {
                return (null, "Позволени формати: JPG, PNG, GIF, WEBP.");
            }

            var uploadsFolder = Path.Combine(_environment.WebRootPath, "images");
            Directory.CreateDirectory(uploadsFolder);

            var uniqueFileName = Guid.NewGuid().ToString("N") + extension.ToLowerInvariant();
            var filePath = Path.Combine(uploadsFolder, uniqueFileName);

            await using var fileStream = new FileStream(filePath, FileMode.Create);
            await imageFile.CopyToAsync(fileStream);

            return ("/images/" + uniqueFileName, null);
        }
    }
}
