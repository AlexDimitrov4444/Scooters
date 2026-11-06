using Microsoft.AspNetCore.Mvc;
using Scooters.Models;
using Scooters.Services;

namespace Scooters.Controllers
{
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;

        public CartController(CartService cartService, ProductService productService)
        {
            _cartService = cartService;
            _productService = productService;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var cart = _cartService.GetCart();
            ViewData["Total"] = _cartService.GetTotal();
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddToCart(Guid productId)
        {
            var product = await _productService.GetProductByIdAsync(productId);
            if (product == null) return NotFound();

            var item = new CartItem
            {
                ProductId = product.Id,
                Name = product.Name,
                Price = product.Price,
                Quantity = 1
            };
            _cartService.AddToCart(item);
            return RedirectToAction("Index", "Cart");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(Guid productId)
        {
            _cartService.RemoveFromCart(productId);
            return RedirectToAction(nameof(Index));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            _cartService.ClearCart();
            return RedirectToAction(nameof(Index));
        }
    }
}
