using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Scooters.Data
{
    public class AppDbContextFactory : IDesignTimeDbContextFactory<AppDbContext>
    {
        public AppDbContext CreateDbContext(string[] args)
        {
            var optionsBuilder = new DbContextOptionsBuilder<AppDbContext>();
            var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Database");
            Directory.CreateDirectory(dataDir);
            var dbPath = Path.Combine(dataDir, "scooters.db");
            optionsBuilder.UseSqlite($"Data Source={dbPath}");
            return new AppDbContext(optionsBuilder.Options);
        }
    }
}
