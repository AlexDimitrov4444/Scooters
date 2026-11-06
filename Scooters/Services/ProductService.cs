using Microsoft.EntityFrameworkCore;
using Scooters.Data;
using Scooters.Models;

namespace Scooters.Services
{
    // ProductService съдържа бизнес логиката за работа с продукти
    // Контролерите не достъпват базата директно — минават през тази услуга
    public class ProductService
    {
        // Logger за записване на информационни съобщения (кой продукт е добавен/изтрит)
        private readonly ILogger<ProductService> _logger;

        // DbContext — връзката с базата данни
        private readonly AppDbContext _context;

        // Конструкторът получава зависимостите чрез dependency injection
        public ProductService(ILogger<ProductService> logger, AppDbContext context)
        {
            _logger = logger;
            _context = context;
        }

        public async Task<List<Product>> GetAllProductsAsync(string? search = null, string? sort = null)
        {
            var query = _context.Products.AsQueryable();

            if (!string.IsNullOrWhiteSpace(search))
            {
                var term = search.Trim();
                query = query.Where(p =>
                    p.Name.Contains(term) ||
                    (p.Description != null && p.Description.Contains(term)));
            }

            query = sort switch
            {
                "price_asc" => query.OrderBy(p => p.Price),
                "price_desc" => query.OrderByDescending(p => p.Price),
                "range" => query.OrderByDescending(p => p.RangeKm),
                "speed" => query.OrderByDescending(p => p.MaxSpeedKmh),
                _ => query.OrderBy(p => p.Name)
            };

            return await query.ToListAsync();
        }

        // Синхронна версия — връща всички продукти (използва се когато async не е нужен)
        public List<Product> GetAllProducts()
        {
            return _context.Products.ToList();
        }

        // Търси продукт по ID асинхронно — връща null ако не е намерен
        public async Task<Product?> GetProductByIdAsync(Guid id)
        {
            return await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
        }

        // Синхронна версия — хвърля изключение ако продуктът не съществува
        public Product GetProductById(Guid id)
        {
            var product = _context.Products.FirstOrDefault(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }
            return product;
        }

        // Добавя нов продукт в базата данни асинхронно
        public async Task<Product> AddProductAsync(Product product)
        {
            // Проверка за null — не можем да добавим несъществуващ обект
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            // Създаваме нов обект с нов GUID — не доверяваме на подадения ID от формата
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

            // Записваме в лога кой продукт е добавен
            _logger.LogInformation("Adding product: {ProductName} with price {ProductPrice}", toAdd.Name, toAdd.Price);

            // Добавяме продукта към DbSet и записваме в базата
            _context.Products.Add(toAdd);
            await _context.SaveChangesAsync();
            return toAdd;
        }

        // Синхронна версия на AddProductAsync
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

        // Обновява съществуващ продукт в базата данни асинхронно
        public async Task<Product> UpdateProductAsync(Product product)
        {
            if (product == null)
            {
                throw new ArgumentNullException(nameof(product));
            }

            // Намираме оригиналния запис в базата по ID
            var existing = await _context.Products.FirstOrDefaultAsync(p => p.Id == product.Id);
            if (existing == null)
            {
                throw new KeyNotFoundException($"Product with ID {product.Id} not found");
            }

            // Обновяваме полетата на намерения запис
            existing.Name = product.Name;
            existing.Price = product.Price;
            existing.Description = product.Description;
            existing.RangeKm = product.RangeKm;
            existing.MaxSpeedKmh = product.MaxSpeedKmh;

            // Обновяваме снимката само ако е подадена нова (null означава "запази старата")
            if (product.ImageUrl != null)
            {
                existing.ImageUrl = product.ImageUrl;
            }

            _logger.LogInformation("Updating product: {ProductName}", existing.Name);

            // Записваме промените в базата
            await _context.SaveChangesAsync();
            return existing;
        }

        // Изтрива продукт по ID асинхронно
        public async Task DeleteProductAsync(Guid id)
        {
            // Намираме продукта — ако не съществува, хвърляме изключение
            var product = await _context.Products.FirstOrDefaultAsync(p => p.Id == id);
            if (product == null)
            {
                throw new KeyNotFoundException($"Product with ID {id} not found");
            }

            // Маркираме за изтриване и записваме в базата
            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
        }

        // Синхронна версия на DeleteProductAsync
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
