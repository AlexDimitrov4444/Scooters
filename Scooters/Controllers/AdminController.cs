using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Scooters.Data;
using Scooters.Models;

namespace Scooters.Controllers
{
    [Authorize(Policy = "AdminOnly")]
    [Route("[controller]/[action]")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;

        public AdminController(AppDbContext context)
        {
            _context = context;
        }

        // GET /Admin — дашборд
        [HttpGet("/Admin")]
        public async Task<IActionResult> Index()
        {
            var orders = await _context.Orders.ToListAsync();
            ViewData["TotalOrders"] = orders.Count;
            ViewData["TotalRevenue"] = orders.Where(o => o.Status != OrderStatus.Cancelled).Sum(o => o.Total);
            ViewData["Pending"] = orders.Count(o => o.Status == OrderStatus.Pending);
            ViewData["Processing"] = orders.Count(o => o.Status == OrderStatus.Processing);
            ViewData["Shipped"] = orders.Count(o => o.Status == OrderStatus.Shipped);
            ViewData["Delivered"] = orders.Count(o => o.Status == OrderStatus.Delivered);
            ViewData["Cancelled"] = orders.Count(o => o.Status == OrderStatus.Cancelled);

            var recent = await _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt)
                .Take(5)
                .ToListAsync();

            return View(recent);
        }

        // GET /Admin/Orders — всички поръчки
        [HttpGet]
        public async Task<IActionResult> Orders(int page = 1)
        {
            const int pageSize = 15;
            var query = _context.Orders
                .Include(o => o.User)
                .OrderByDescending(o => o.CreatedAt);

            var total = await query.CountAsync();
            var items = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            ViewData["Page"] = page;
            ViewData["TotalPages"] = (int)Math.Ceiling(total / (double)pageSize);
            return View(items);
        }

        // GET /Admin/Orders/{id}
        [HttpGet("{id}")]
        public async Task<IActionResult> OrderDetails(Guid id)
        {
            var order = await _context.Orders
                .Include(o => o.User)
                .Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null) return NotFound();
            return View(order);
        }

        // POST /Admin/Orders/{id}/Status
        [HttpPost("{id}")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(Guid id, OrderStatus status)
        {
            var order = await _context.Orders.FindAsync(id);
            if (order == null) return NotFound();

            order.Status = status;
            await _context.SaveChangesAsync();
            TempData["Success"] = "Статусът е обновен.";
            return RedirectToAction(nameof(OrderDetails), new { id });
        }

        // GET /Admin/Products
        [HttpGet]
        public async Task<IActionResult> Products()
        {
            var products = await _context.Products.OrderBy(p => p.Name).ToListAsync();
            return View(products);
        }

        // GET /Admin/Users
        [HttpGet]
        public async Task<IActionResult> Users()
        {
            var users = await _context.Users
                .Select(u => new
                {
                    u.Id, u.FullName, u.Email, u.IsAdmin,
                    OrderCount = _context.Orders.Count(o => o.UserId == u.Id)
                })
                .ToListAsync();

            return View(users.Select(u => new AdminUserViewModel
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                IsAdmin = u.IsAdmin,
                OrderCount = u.OrderCount
            }).ToList());
        }
    }
}
