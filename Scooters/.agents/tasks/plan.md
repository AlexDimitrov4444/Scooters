# Implementation Plan

## Context

- **Project**: ASP.NET Core 8 MVC — `MVC.Intro` namespace, target `net8.0`
- **Build**: `dotnet build` from `c:\Users\Students\MVC\MVC\MVC.Intro`
- **Run/smoke-test**: `dotnet run` — app starts, navigates to `/Product` and `/Cart`
- **No automated test project** present; verification is build + manual smoke checks
- **Key facts discovered**:
  - `Program.cs` hardcodes `Path.Combine(...ContentRootPath, "Data")` to locate the DB — must be updated alongside `appsettings.json`
  - `AppDbContextFactory.cs` also hardcodes `"Data"` — used by EF migrations CLI
  - `Product.Id` is `Guid`, `Price` is `decimal`
  - `Services/` folder already exists (`ProductService.cs`, `ImageUploadService.cs`) — `CartService` goes there
  - `_ViewImports.cshtml` already imports `MVC.Intro.Models`, so `CartItem` is available in views without extra `@using`
  - Layout uses Bootstrap 5; nav links follow the `asp-controller` / `asp-action` tag-helper pattern with an active-class inline check
  - `[Route("[controller]/[action]")]` is on `ProductController`; `CartController` should use the same convention
  - No session middleware is registered yet — `AddSession`, `AddDistributedMemoryCache`, `AddHttpContextAccessor`, and `UseSession()` are all missing

---

## Task 1 — Move SQLite DB to `Database\` folder

- [ ] 1. Create the `Database\` folder, move `scooters.db` into it, and update `appsettings.json`.
      - Create directory `c:\Users\Students\MVC\MVC\MVC.Intro\Database\`.
      - Move `Data\scooters.db` → `Database\scooters.db`.
      - In `appsettings.json` change `"Data Source=Data/scooters.db"` to `"Data Source=Database/scooters.db"`.  
        (Note: this connection string is present in `appsettings.json` but **not** consumed by `Program.cs` at runtime — `Program.cs` builds its own path; the string is kept for documentation/tooling consistency.)
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Database\` (new folder), `appsettings.json`, `Data\scooters.db` (moved)
      - Verify: `dotnet build` passes with no errors.

- [ ] 2. Update the runtime DB path in `Program.cs`.
      - Change the line  
        `var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Data");`  
        to  
        `var dataDir = Path.Combine(builder.Environment.ContentRootPath, "Database");`  
      - The `Directory.CreateDirectory(dataDir)` call immediately after it stays unchanged — it will now create `Database\` if it doesn't exist.
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Program.cs`
      - Verify: `dotnet build` passes; `dotnet run` starts without DB errors and `/Product` loads data.

- [ ] 3. Update the design-time DB path in `AppDbContextFactory.cs`.
      - Change  
        `var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Data");`  
        to  
        `var dataDir = Path.Combine(Directory.GetCurrentDirectory(), "Database");`  
      - This factory is used only by `dotnet ef` CLI commands (migrations); it must match the new folder so future migrations work.
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Data\AppDbContextFactory.cs`
      - Verify: `dotnet build` passes. (Full EF migration tooling can be confirmed with `dotnet ef dbcontext info` from the project directory, but build passing is the minimum bar.)

---

## Task 2 — Session-based shopping cart

- [ ] 4. Enable session infrastructure in `Program.cs`.
      - **After** `builder.Services.AddControllersWithViews()` add:
        ```csharp
        builder.Services.AddDistributedMemoryCache();   // in-process session store
        builder.Services.AddHttpContextAccessor();
        builder.Services.AddSession(options =>
        {
            options.IdleTimeout = TimeSpan.FromMinutes(30);
            options.Cookie.HttpOnly = true;
            options.Cookie.IsEssential = true;
        });
        ```
      - In the middleware pipeline, add `app.UseSession();` **after** `app.UseRouting()` and **before** `app.UseAuthentication()`.
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Program.cs`
      - Verify: `dotnet build` passes.

- [ ] 5. Create the `CartItem` model.
      - New file `Models\CartItem.cs` in namespace `MVC.Intro.Models`:
        ```csharp
        namespace MVC.Intro.Models
        {
            public class CartItem
            {
                public Guid ProductId { get; set; }
                public string Name { get; set; } = string.Empty;
                public decimal Price { get; set; }
                public string? ImageUrl { get; set; }
                public int Quantity { get; set; }
            }
        }
        ```
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Models\CartItem.cs`
      - Verify: `dotnet build` passes.

- [ ] 6. Create `CartService` in `Services\CartService.cs`.
      - Namespace `MVC.Intro.Services`. Uses `IHttpContextAccessor` and `System.Text.Json`.
      - Session key constant: `private const string CartKey = "Cart";`
      - Private helpers: `GetCart()` — deserializes `List<CartItem>` from session (returns empty list if missing); `SaveCart(List<CartItem>)` — serializes back.
      - Public methods:
        - `List<CartItem> GetCart()` — returns current cart items.
        - `void AddItem(Product product)` — finds existing item by `ProductId` and increments `Quantity`, or adds new `CartItem` with `Quantity = 1`.
        - `void RemoveItem(Guid productId)` — removes the item with matching `ProductId`.
        - `void Clear()` — removes the `"Cart"` key from session.
        - `decimal GetTotal()` — returns `cart.Sum(i => i.Price * i.Quantity)`.
      - Inject `IHttpContextAccessor`; access session via `_httpContextAccessor.HttpContext!.Session`.
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Services\CartService.cs`
      - Verify: `dotnet build` passes.

- [ ] 7. Register `CartService` as scoped in `Program.cs`.
      - Add `builder.Services.AddScoped<CartService>();` alongside the other `AddScoped` calls.
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Program.cs`
      - Verify: `dotnet build` passes.

- [ ] 8. Create `CartController`.
      - New file `Controllers\CartController.cs`, namespace `MVC.Intro.Controllers`.
      - Decorate with `[Route("[controller]/[action]")]` — same convention as `ProductController`.
      - Constructor injects `CartService`.
      - Actions:
        - `[HttpGet] Index()` → passes `cartService.GetCart()` as model to `View()`, also puts `cartService.GetTotal()` in `ViewData["Total"]`.
        - `[HttpPost] [ValidateAntiForgeryToken] AddToCart(Guid productId)` — looks up the product via a `ProductService` injection, calls `cartService.AddItem(product)`, redirects to `Product/Index`.
        - `[HttpPost] [ValidateAntiForgeryToken] RemoveFromCart(Guid productId)` — calls `cartService.RemoveItem(productId)`, redirects to `Cart/Index`.
        - `[HttpPost] [ValidateAntiForgeryToken] ClearCart()` — calls `cartService.Clear()`, redirects to `Cart/Index`.
      - `ProductService` must also be injected (to look up the product in `AddToCart`).
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Controllers\CartController.cs`
      - Verify: `dotnet build` passes.

- [ ] 9. Add "Добави в кошница" button to the product list view.
      - In `Views\Product\Index.cshtml`, inside each card's `<div class="card-body">`, **after** the "Детайли" button, add:
        ```html
        <form asp-controller="Cart" asp-action="AddToCart" method="post" class="d-inline">
            <input type="hidden" name="productId" value="@item.Id" />
            <button type="submit" class="btn btn-sm btn-success">Добави в кошница</button>
        </form>
        ```
      - No `[ValidateAntiForgeryToken]` token tag is needed in the form — the tag helper `asp-action` automatically injects `__RequestVerificationToken` when inside a `<form>` with `method="post"`.
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Views\Product\Index.cshtml`
      - Verify: `dotnet build` passes; `/Product` page renders the new button per card.

- [ ] 10. Create the Cart `Index` view.
      - New file `Views\Cart\Index.cshtml`. Model is `IEnumerable<CartItem>` (already in `_ViewImports`).
      - Layout: `@{ ViewData["Title"] = "Кошница"; Layout = "_Layout"; }`
      - Content:
        - `<h1>Кошница 🛒</h1>`
        - If cart is empty: show a message "Кошницата е празна." and a link back to `/Product/Index`.
        - If not empty: Bootstrap table with columns — Продукт, Снимка (thumbnail 60px), Цена, Брой, Сума, Изтрий.
          - Each row's "Изтрий" cell: POST form to `Cart/RemoveFromCart` with hidden `productId`.
          - Below the table: total row showing `ViewData["Total"]` formatted as `0.00 лв.`
          - Two action buttons: "Изчисти кошницата" (POST form to `Cart/ClearCart`) and "Продължи пазаруването" link back to `Product/Index`.
        - All POST forms include `@Html.AntiForgeryToken()` (or use `asp-action` tag helpers which inject it automatically).
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Views\Cart\` (new folder), `Views\Cart\Index.cshtml`
      - Verify: `dotnet build` passes; navigating to `/Cart/Index` renders without errors.

- [ ] 11. Add "Кошница 🛒" nav link to the layout.
      - In `Views\Shared\_Layout.cshtml`, in the left `<ul class="navbar-nav flex-grow-1">`, add a new `<li>` after the "Тротинетки" item:
        ```html
        <li class="nav-item">
            <a class="nav-link @(ViewContext.RouteData.Values["controller"]?.ToString() == "Cart" ? "active" : null)"
               asp-controller="Cart" asp-action="Index">Кошница 🛒</a>
        </li>
        ```
      - Files: `c:\Users\Students\MVC\MVC\MVC.Intro\Views\Shared\_Layout.cshtml`
      - Verify: `dotnet build` passes; the navbar shows "Кошница 🛒" on every page and it becomes active when on the cart page.

---

## Final smoke test

After all items are complete, run `dotnet run` from the project directory and confirm:

1. App starts without errors.
2. `/Product` lists scooters; each card has "Добави в кошница".
3. Clicking "Добави в кошница" adds the product and redirects back to `/Product`.
4. `/Cart/Index` shows the item, correct price, and total.
5. "Изтрий" removes the single item; "Изчисти кошницата" empties the whole cart.
6. The "Кошница 🛒" nav link is visible and marks active on the cart page.
7. DB file is at `Database\scooters.db` and data persists across restarts.
