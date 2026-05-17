# EcommersShop

A full-stack e-commerce web application built with ASP.NET Core 10.0, following a three-layer architecture (DAL / BLL / Web).

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 10.0 (MVC + Razor Pages) |
| ORM | Entity Framework Core 10 |
| Database | SQL Server (LocalDB) |
| Auth | Cookie Authentication + BCrypt |
| Mapping | AutoMapper 12 |
| UI | Bootstrap 5.3, Bootstrap Icons |
| API Docs | Swagger / Swashbuckle |
| Localization | ASP.NET Core IStringLocalizer (RU / AZ / EN) |

---

## Features

### Buyers
- Browse product catalog with category and brand filters, sorting
- Product detail page with images and ratings
- Shopping cart (add, update quantity, remove, clear)
- Checkout and order placement with coupon support
- Order history with status tracking
- Order cancellation within 1 hour of placement (live countdown timer)
- Wishlist / favorites
- Delivery address management
- Profile page

### Sellers
- Seller registration (requires admin approval)
- Add products with visual category icon picker
- Products go through admin review before going live
- Seller dashboard with personal sales statistics

### Admins
- Admin dashboard with charts (Chart.js)
- User management (activate / deactivate sellers)
- Product approval (activate / deactivate)
- Category, brand, coupon CRUD
- Order management

### General
- Multi-language support: Russian 🇷🇺, Azerbaijani 🇦🇿, English 🇬🇧
- Language switcher in the navbar — persisted via cookie
- Responsive layout (Bootstrap 5.3)
- REST API with Swagger UI

---

## Project Structure

```
EcommersProject/
├── EcommersProject.DAL/        # Entities, DbContext, Repositories
├── EcommersProject.BLL/        # DTOs, Services, Interfaces, AutoMapper
├── EcommersProject/            # ASP.NET Core Web App
│   ├── Controllers/            # MVC Controllers (Shop, Cart, Account, etc.)
│   ├── Pages/                  # Razor Pages (Admin, Seller panels)
│   ├── Views/                  # Razor Views
│   ├── Resources/              # Localization .resx files (ru / az / en)
│   └── Program.cs
└── EcommersProject.API/        # Separate REST API project
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server LocalDB

### Run

```bash
git clone https://github.com/natigea/FinallyProject.git
cd FinallyProject
dotnet run --project EcommersProject/EcommersProject.csproj
```

The app seeds the database with categories, brands, and sample products on first run.

### Default Admin Account

| Field | Value |
|-------|-------|
| Email | admin@ecommer.com |
| Password | Admin123! |

---

## User Roles

| Role | Description |
|------|-------------|
| **Customer** | Can browse, buy, manage orders |
| **Seller** | Can add products (pending admin approval) |
| **Admin** | Full access to admin panel |

---

## Screenshots

> Home page, catalog, cart, orders, and admin panel included.

---

## License

Educational project — Code Academy Baku, 2026.
