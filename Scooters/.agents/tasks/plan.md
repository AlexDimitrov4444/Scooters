# Implementation Plan — Scooters Admin & Client Panels

Project: `c:\Users\Students\MVC\MVC\Scooters\Scooters.csproj`  
Build: `dotnet build c:\Users\Students\MVC\MVC\Scooters\Scooters.csproj`  
EF migrations: `dotnet ef migrations add <Name> --project c:\Users\Students\MVC\MVC\Scooters`

---

## FEAT-001 — Admin Role + Order Models + Migrations

- [ ] 1. Add `public bool IsAdmin { get; set; }` to `Models/User.cs`.
      No annotation needed — EF maps bool as non-nullable.
      Files: `Models/User.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 2. Update `Data/DbSeeder.cs`: set demo.IsAdmin = false; add admin@scooters.bg / Admin123! user with IsAdmin=true. Guard with `if (!context.Users.Any(u => u.Email == "admin@scooters.bg"))`.
      Files: `Data/DbSeeder.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 3. Edit `Controllers/AccountController.cs` → `SignInUser`: after claims list, add `if (user.IsAdmin) claims.Add(new Claim(ClaimTypes.Role, "Admin"));`.
      Files: `Controllers/AccountController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 4. Edit `Program.cs`: change `AddAuthorization()` to `AddAuthorization(options => { options.AddPolicy("AdminOnly", policy => policy.RequireRole("Admin")); })`.
      Files: `Program.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 5. Create `Models/OrderStatus.cs`: enum with Pending, Processing, Shipped, Delivered, Cancelled.
      Files: `Models/OrderStatus.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 6. Create `Models/Order.cs`: Id Guid, UserId Guid, User nav, CreatedAt DateTime, Status OrderStatus, FullName/Phone/Address/City strings with [Required][MaxLength], Total decimal, Items List<OrderItem>.
      Files: `Models/Order.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 7. Create `Models/OrderItem.cs`: Id Guid, OrderId Guid, Order nav, ProductId Guid, ProductName string, UnitPrice decimal, Quantity int, [NotMapped] computed Subtotal property.
      Files: `Models/OrderItem.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 8. Create `Models/CheckoutViewModel.cs`: FullName/Phone/Address/City all [Required] with Bulgarian display names, Items List<CartItem>, Total decimal.
      Files: `Models/CheckoutViewModel.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 9. Edit `Data/AppDbContext.cs`: add DbSet<Order> and DbSet<OrderItem>. In OnModelCreating configure Order→User FK with DeleteBehavior.Restrict, cascade delete for OrderItems. Add `[NotMapped]` attribute to OrderItem.Subtotal or configure via Fluent.
      Files: `Data/AppDbContext.cs`, `Models/OrderItem.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 10. Run EF migration for IsAdmin: `dotnet ef migrations add AddIsAdminToUser --project c:\Users\Students\MVC\MVC\Scooters`
      Files: `Migrations/` (new AddIsAdminToUser files + updated snapshot)
      Verify: Migration files created, `dotnet build` — Build succeeded.

- [ ] 11. Run EF migration for Orders: `dotnet ef migrations add AddOrders --project c:\Users\Students\MVC\MVC\Scooters`
      Files: `Migrations/` (new AddOrders files + updated snapshot)
      Verify: Migration files created, `dotnet build` — Build succeeded.

---

## FEAT-002 — Checkout Flow + Client Panel

- [ ] 12. Edit `Controllers/CartController.cs`: inject AppDbContext, add `using` for System.Security.Claims, Scooters.Data, Microsoft.AspNetCore.Authorization, Microsoft.EntityFrameworkCore.
      Files: `Controllers/CartController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 13. Add GET /Cart/Checkout: [Authorize][HttpGet]. If cart empty → TempData["Error"] + redirect to Index. Build CheckoutViewModel (FullName from ClaimTypes.Name, Items=cart, Total). Return View.
      Files: `Controllers/CartController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 14. Add POST /Cart/Checkout: [Authorize][HttpPost][ValidateAntiForgeryToken]. Validate model, create Order + OrderItems, SaveChangesAsync, ClearCart, redirect to OrderConfirmation(order.Id).
      Files: `Controllers/CartController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 15. Add GET /Cart/OrderConfirmation/{id}: [Authorize][HttpGet]. Query Order with Items included, verify ownership (UserId == currentUser), return View(order).
      Files: `Controllers/CartController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 16. Create `Views/Cart/Checkout.cshtml`: @model CheckoutViewModel. Two-column layout (col-md-7 form / col-md-5 order summary card). Form fields: FullName, Phone, Address, City with asp-for helpers + validation spans. Submit button "Потвърди поръчката" (btn-warning). Right column: items table + total. Include _ValidationScriptsPartial in @section Scripts.
      Files: `Views/Cart/Checkout.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 17. Create `Views/Cart/OrderConfirmation.cshtml`: @model Order. Success alert with order number (Id.ToString()[..8]). Shipping info card. Items table (Продукт/Цена/Кол./Сума). Total. Links to Account/Orders and Product/Index.
      Files: `Views/Cart/OrderConfirmation.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 18. Edit `Views/Cart/Index.cshtml`: add "Към плащане →" button (btn-warning) visible only to authenticated users, linking to Cart/Checkout.
      Files: `Views/Cart/Index.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 19. Edit `Controllers/AccountController.cs`: add GET /Account/Orders [Authorize] — list current user's orders newest-first. Add GET /Account/Orders/{id} [Authorize] — detail with items, ownership check.
      Files: `Controllers/AccountController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 20. Create `Views/Account/Orders.cshtml`: @model List<Order>. Title "Моите поръчки". Table: Номер/Дата/Статус badge/Сума/Детайли link. Status badge colors: Pending=secondary, Processing=primary, Shipped=info, Delivered=success, Cancelled=danger.
      Files: `Views/Account/Orders.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 21. Create `Views/Account/OrderDetails.cshtml`: @model Order. Title "Поръчка #…". Status badge. Shipping info. Items table. Total. Back link.
      Files: `Views/Account/OrderDetails.cshtml`
      Verify: `dotnet build` — Build succeeded.

---

## FEAT-003 — Admin Panel + Navigation

- [ ] 22. Create `Models/UserSummaryViewModel.cs`: Id Guid, FullName/Email strings, IsAdmin bool, OrderCount int.
      Files: `Models/UserSummaryViewModel.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 23. Create `Controllers/AdminController.cs`: [Authorize(Policy="AdminOnly")][Route("[controller]/[action]")]. Inject AppDbContext. Add GET Index action: aggregates (total orders, revenue, by-status counts), recent 5 orders. Pass all via ViewBag.
      Files: `Controllers/AdminController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 24. Add GET Admin/Orders (paginated): page param, 20 per page, orders with User included, ordered by CreatedAt desc. ViewBag: Orders, CurrentPage, TotalPages.
      Files: `Controllers/AdminController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 25. Add GET Admin/Orders/{id} (detail) and POST Admin/Orders/{id}/Status: detail includes Items + User; POST updates Status, saves, TempData["Success"], redirects.
      Files: `Controllers/AdminController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 26. Add GET Admin/Products: returns all products. Add GET Admin/Users: projects to List<UserSummaryViewModel> with order count subquery.
      Files: `Controllers/AdminController.cs`
      Verify: `dotnet build` — Build succeeded.

- [ ] 27. Create `Views/Admin/Index.cshtml`: 4 stat cards (Total orders, Revenue, Active, Delivered). Recent 5 orders table. All labels Bulgarian.
      Files: `Views/Admin/Index.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 28. Create `Views/Admin/Orders.cshtml`: paginated table (Номер/Клиент/Дата/Статус badge/Сума/Действия). Prev/Next pagination controls.
      Files: `Views/Admin/Orders.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 29. Create `Views/Admin/OrderDetails.cshtml`: @model Order. Shipping info, items table, status change form (select + POST to Admin/Orders/{id}/Status), status badge.
      Files: `Views/Admin/OrderDetails.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 30. Create `Views/Admin/Products.cshtml`: @model List<Product>. Table with thumbnail, name, price, range, speed, Edit/Delete actions.
      Files: `Views/Admin/Products.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 31. Create `Views/Admin/Users.cshtml`: @model List<UserSummaryViewModel>. Table: Имена/Имейл/Администратор badge/Поръчки count.
      Files: `Views/Admin/Users.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 32. Edit `Views/Shared/_Layout.cshtml`: (a) Add `@inject Scooters.Services.CartService CartSvc` at top. (b) Update Cart nav-item to show count badge: `<span class="badge bg-warning text-dark">@(CartSvc.GetCart().Count)</span>`. (c) In authenticated nav block add "Моите поръчки" link to Account/Orders. (d) Add `@if (User.IsInRole("Admin"))` block with "Админ панел" link to Admin/Index.
      Files: `Views/Shared/_Layout.cshtml`
      Verify: `dotnet build` — Build succeeded.

- [ ] 33. Final full build and smoke test: run `dotnet build`, confirm 0 errors; run `dotnet run` briefly to confirm startup and migration apply without exceptions.
      Files: (none)
      Verify: `dotnet build c:\Users\Students\MVC\MVC\Scooters\Scooters.csproj` → Build succeeded.
