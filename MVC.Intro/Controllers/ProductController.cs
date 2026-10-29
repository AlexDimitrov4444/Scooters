using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MVC.Intro.Models;
using MVC.Intro.Services;

namespace MVC.Intro.Controllers
{
    // ProductController управлява всички действия свързани с продуктите (тротинетките)
    // Маршрутът е: /Product/{action}/{id?}
    [Route("[controller]/[action]")]
    public class ProductController : Controller
    {
        // Бизнес логиката за продукти — инжектира се автоматично
        private readonly ProductService _productService;

        // Предоставя достъп до wwwroot папката за качване на снимки
        private readonly IWebHostEnvironment _webHostEnvironment;

        // Конструктор — зависимостите се подават от dependency injection контейнера
        public ProductController(ProductService productService, IWebHostEnvironment webHostEnvironment)
        {
            _productService = productService;
            _webHostEnvironment = webHostEnvironment;
        }

        // GET /Product/Index — показва списък с всички продукти
        public async Task<IActionResult> Index()
        {
            // Зареждаме всички продукти от базата и ги подаваме към view-то
            var products = await _productService.GetAllProductsAsync();
            return View(products);
        }

        // GET /Product/Details/{id} — показва детайлна информация за един продукт
        [HttpGet("{id}")]
        public async Task<IActionResult> Details(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);

            // Ако продуктът не съществува, връщаме 404
            if (product == null)
            {
                return NotFound();
            }
            return View(product);
        }

        // GET /Product/Create — показва формата за добавяне на нов продукт
        // [Authorize] — само влезли потребители имат достъп
        [Authorize]
        [HttpGet]
        public IActionResult Create()
        {
            return View();
        }

        // POST /Product/Create — обработва изпратената форма за нов продукт
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken] // Защита срещу CSRF атаки
        public async Task<IActionResult> Create(Product product, IFormFile? imageFile)
        {
            // Проверяваме дали данните от формата са валидни (според атрибутите в модела)
            if (!ModelState.IsValid)
            {
                // Ако има грешки, показваме формата отново с грешките
                return View(product);
            }

            // Обработваме качената снимка (ако е подадена)
            if (imageFile != null && imageFile.Length > 0)
            {
                // Папката за качване: wwwroot/images
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");

                // Създаваме папката ако не съществува
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                // Генерираме уникално име на файла (GUID + оригинално разширение)
                // Това предотвратява презаписване на съществуващи файлове
                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                // Записваме файла на диска
                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Запазваме относителния URL за ползване в <img> тагове
                product.ImageUrl = "/images/" + uniqueFileName;
            }

            // Записваме продукта в базата данни
            await _productService.AddProductAsync(product);

            // Пренасочваме към списъка с продукти
            return RedirectToAction(nameof(Index));
        }

        // GET /Product/Edit/{id} — показва формата за редактиране на продукт
        [Authorize]
        [HttpGet("{id}")]
        public async Task<IActionResult> Edit(Guid id)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }
            // Подаваме съществуващите данни към формата
            return View(product);
        }

        // POST /Product/Edit/{id} — записва промените по продукта
        [Authorize]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(Guid id, Product product, IFormFile? imageFile)
        {
            // Проверка: ID от URL трябва да съвпада с ID от формата
            if (id != product.Id)
            {
                return BadRequest();
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            // Ако е качена нова снимка, я записваме и обновяваме URL-а
            if (imageFile != null && imageFile.Length > 0)
            {
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                if (!Directory.Exists(uploadsFolder))
                {
                    Directory.CreateDirectory(uploadsFolder);
                }

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                product.ImageUrl = "/images/" + uniqueFileName;
            }

            try
            {
                // Обновяваме продукта в базата
                await _productService.UpdateProductAsync(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            return RedirectToAction(nameof(Index));
        }

        // POST /Product/UploadImage/{id} — качва само снимката на продукт (от Home страницата)
        // Използва се от бутона "📷 Смени снимка" върху картите
        [Authorize]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile? imageFile)
        {
            if (imageFile != null && imageFile.Length > 0)
            {
                // Записваме новата снимка в wwwroot/images
                var uploadsFolder = Path.Combine(_webHostEnvironment.WebRootPath, "images");
                Directory.CreateDirectory(uploadsFolder);

                var uniqueFileName = Guid.NewGuid().ToString() + Path.GetExtension(imageFile.FileName);
                var filePath = Path.Combine(uploadsFolder, uniqueFileName);

                using (var fileStream = new FileStream(filePath, FileMode.Create))
                {
                    await imageFile.CopyToAsync(fileStream);
                }

                // Намираме продукта и обновяваме само ImageUrl
                var product = await _productService.GetProductByIdAsync(id);
                if (product == null) return NotFound();

                product.ImageUrl = "/images/" + uniqueFileName;
                await _productService.UpdateProductAsync(product);
            }

            // Връщаме се на началната страница след качването
            return RedirectToAction("Index", "Home");
        }

        // POST /Product/Delete — изтрива продукт по ID
        [Authorize]
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                await _productService.DeleteProductAsync(id);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            // След изтриването се връщаме към списъка
            return RedirectToAction(nameof(Index));
        }
    }
}
