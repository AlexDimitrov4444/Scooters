using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scooters.Data;
using Scooters.Models;
using Scooters.Services;

namespace Scooters.Controllers
{
    [Route("[controller]/[action]")]
    public class CartController : Controller
    {
        private readonly CartService _cartService;
        private readonly ProductService _productService;
        private readonly AppDbContext _context;

        public CartController(CartService cartService, ProductService productService, AppDbContext context)
        {
            _cartService = cartService;
            _productService = productService;
            _context = context;
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

        // GET /Cart/Checkout — форма за доставка
        [HttpGet]
        [Authorize]
        public IActionResult Checkout()
        {
            var cart = _cartService.GetCart();
            if (!cart.Any())
                return RedirectToAction(nameof(Index));

            var userName = User.FindFirstValue(ClaimTypes.Name) ?? string.Empty;
            var vm = new CheckoutViewModel
            {
                FullName = userName,
                Items = cart,
                Total = _cartService.GetTotal()
            };
            return View(vm);
        }

        // POST /Cart/Checkout — запис на поръчката
        [HttpPost]
        [Authorize]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Checkout(CheckoutViewModel vm)
        {
            var cart = _cartService.GetCart();
            if (!cart.Any())
                return RedirectToAction(nameof(Index));

            vm.Items = cart;
            vm.Total = _cartService.GetTotal();

            if (!ModelState.IsValid)
                return View(vm);

            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var order = new Order
            {
                Id = Guid.NewGuid(),
                UserId = userId,
                CreatedAt = DateTime.UtcNow,
                Status = OrderStatus.Pending,
                FullName = vm.FullName,
                Phone = vm.Phone,
                Address = vm.Address,
                City = vm.City,
                Total = vm.Total,
                Items = cart.Select(c => new OrderItem
                {
                    Id = Guid.NewGuid(),
                    ProductId = c.ProductId,
                    ProductName = c.Name,
                    UnitPrice = c.Price,
                    Quantity = c.Quantity
                }).ToList()
            };

            _context.Orders.Add(order);
            await _context.SaveChangesAsync();
            _cartService.ClearCart();

            return RedirectToAction(nameof(OrderConfirmation), new { id = order.Id });
        }

        // GET /Cart/OrderConfirmation/{id}
        [HttpGet("{id}")]
        [Authorize]
        public async Task<IActionResult> OrderConfirmation(Guid id)
        {
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            Guid.TryParse(userIdStr, out var userId);

            var order = await _context.Orders
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id && (o.UserId == userId || User.IsInRole("Admin")));

            if (order == null) return NotFound();
            return View(order);
        }
    }
}
