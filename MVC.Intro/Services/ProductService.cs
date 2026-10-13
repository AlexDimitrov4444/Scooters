using Microsoft.EntityFrameworkCore;
using MVC.Intro.Data;
using MVC.Intro.Models;

namespace MVC.Intro.Services
{
    public class ProductService
    {
        private readonly ILogger<ProductService> _logger;
        private readonly AppDbContext _context;

        public ProductService(ILogger<ProductService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync()
        {
            return await _context.Products.ToListAsync();
        }

        public List<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }

        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        public Product GetProductById(Guid id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }
            return product;
        }

        public async Task<Product> AddProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var toAdd = new Product
            {
                Id = Guid.NewGuid(),
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                RangeKm = product.RangeKm,
                MaxSpeedKmh = product.MaxSpeedKmh,
                ImageUrl = product.ImageUrl
            };

            _logger.LogInformation("Adding product: {ProductName} with price {ProductPrice}", toAdd.Name, toAdd.Price);

            _context.Products.Add(toAdd);
            await _context.SaveChangesAsync();
            return toAdd;
        }

        public Product AddProduct(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            var toAdd = new Product
            {
                Id = Guid.NewGuid(),
                Name = product.Name,
                Price = product.Price,
                Description = product.Description,
                RangeKm = product.RangeKm,
                MaxSpeedKmh = product.MaxSpeedKmh,
                ImageUrl = product.ImageUrl
            };

            _logger.LogInformation("Adding product: {ProductName} with price {ProductPrice}", toAdd.Name, toAdd.Price);

            _context.Products.Add(toAdd);
            _context.SaveChanges();
            return toAdd;
        }

        public async Task DeleteProductAsync(Guid id)
        {
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        public void DeleteProduct(Guid id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            _context.Products.Remove(product);
            _context.SaveChanges();
        }
    }
}
