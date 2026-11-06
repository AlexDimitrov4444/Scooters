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
        private readonly ImageUploadService _imageUploadService;

        public ProductController(ProductService productService, ImageUploadService imageUploadService)
        {
            _productService = productService;
            _imageUploadService = imageUploadService;
        }

        public async Task<IActionResult> Index(string? q, string? sort)
        {
            ViewData["Search"] = q ?? string.Empty;
            ViewData["Sort"] = sort ?? "name";
            var products = await _productService.GetAllProductsAsync(q, sort);
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
            if (!await TryAssignImageAsync(product, imageFile))
            {
                return View(product);
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            await _productService.AddProductAsync(product);
            TempData["Success"] = "Моделът е добавен успешно.";
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

            if (!await TryAssignImageAsync(product, imageFile))
            {
                return View(product);
            }

            if (!ModelState.IsValid)
            {
                return View(product);
            }

            try
            {
                await _productService.UpdateProductAsync(product);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }

            TempData["Success"] = "Промените са запазени.";
            return RedirectToAction(nameof(Index));
        }

        // POST /Product/UploadImage/{id} — качва само снимката на продукт (от Home страницата)
        // Използва се от бутона "📷 Смени снимка" върху картите
        [Authorize]
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UploadImage(Guid id, IFormFile? imageFile)
        {
            var product = await _productService.GetProductByIdAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            if (!await TryAssignImageAsync(product, imageFile))
            {
                TempData["Error"] = ModelState.Values.SelectMany(v => v.Errors).FirstOrDefault()?.ErrorMessage
                    ?? "Снимката не беше качена.";
                return RedirectToAction("Index", "Home");
            }

            if (imageFile != null && imageFile.Length > 0)
            {
                await _productService.UpdateProductAsync(product);
                TempData["Success"] = "Снимката е обновена.";
            }

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

            TempData["Success"] = "Моделът е изтрит.";
            return RedirectToAction(nameof(Index));
        }

        private async Task<bool> TryAssignImageAsync(Product product, IFormFile? imageFile)
        {
            var (url, error) = await _imageUploadService.SaveAsync(imageFile);
            if (error != null)
            {
                ModelState.AddModelError("imageFile", error);
                return false;
            }

            if (url != null)
            {
                product.ImageUrl = url;
            }

            return true;
        }
    }
}
