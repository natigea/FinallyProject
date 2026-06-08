# Alış-Veriş — Classifieds Marketplace

A full-stack classifieds marketplace (Avito / OLX style) built with **ASP.NET Core 10.0**, following a three-layer architecture (DAL / BLL / Web) plus a separate REST API project.

---

## Tech Stack

| Layer | Technology |
|-------|-----------|
| Framework | ASP.NET Core 10.0 (MVC + Razor Pages) |
| ORM | Entity Framework Core 10 |
| Database | Azure SQL Server |
| Auth | Cookie Authentication + BCrypt |
| Real-time | SignalR (chat) |
| Push Notifications | Firebase Cloud Messaging (FCM HTTP V1) |
| Voice Calls | Daily.co (audio-only rooms via REST API) |
| Payments | Stripe (Payment Intents) |
| Email | SendGrid API |
| Mapping | AutoMapper 12 |
| UI | Bootstrap 5.3, Bootstrap Icons |
| API Docs | Scalar (separate REST API project with JWT) |
| Localization | ASP.NET Core IStringLocalizer (RU / AZ / EN) |

---

## Features

### Listings
- Browse catalog with full-text search and filters: category, city, price range, VIP-only, has-photo
- Sort by newest or price
- Listing detail page with photo gallery, seller rating and reviews
- Create / edit / delete listings with up to 10 photo uploads
- Listings go through admin moderation before going public
- Close (mark as sold) listings
- VIP promotion: pin listing at the top (7 / 14 / 30 days, paid via Stripe)
- Delivery disabled for categories: Business, Animals, Services, Real Estate, Jobs, Other

### Messaging
- Real-time chat between buyer and seller via **SignalR**
- **FCM push notifications** when the browser tab is closed or in background
- **Voice calls** via Daily.co (audio-only, room per conversation)
- Incoming call screen with accept / decline (SignalR + FCM fallback)
- Call duration logged as a system message in chat
- Read receipts

### Payments (Stripe)
- **Delivery order**: buyer pays listing price + 3% service fee
- Seller receives a notification and can approve or reject the order
- If rejected, listing is automatically reopened
- **VIP promotion**: 7 days · 14 days · 30 days (fixed price tiers)
- Stripe Payment Intents flow (client-side card element → server-side verification)

### Accounts & Notifications
- Registration / login with cookie auth
- Forgot password / reset password flow
- Profile page with sidebar navigation
- In-app notification center (new orders, order status changes, incoming calls)
- FCM device token registration for web push

### Favorites
- Add / remove listings from favorites
- Dedicated favorites page

### Admin Panel (Razor Pages)
- Dashboard with statistics
- User management (activate / deactivate)
- Listing moderation (approve / reject)
- Category CRUD
- Brand CRUD
- Coupon CRUD
- Order management
- Delivery order management

### Seller Panel (Razor Pages)
- Personal sales statistics dashboard
- My listings management
- Incoming delivery order management (approve / reject)

### General
- Multi-language support: Russian, Azerbaijani, English — persisted via cookie
- Responsive layout with Bootstrap 5.3
- Mobile bottom navigation bar
- PWA-ready: FCM service worker for background push notifications

---

## Project Structure

```
Alış-Veriş/
├── EcommersProject.DAL/        # Entities, DbContext, Repositories
│   └── Entities/               # Listing, User, Message, Purchase, Notification …
├── EcommersProject.BLL/        # DTOs, Services, Interfaces, AutoMapper profiles
├── EcommersProject/            # ASP.NET Core Web App
│   ├── Controllers/            # Listing, Messages, Call, Checkout, Account …
│   ├── Pages/                  # Razor Pages: Admin panel, Seller panel, Auth
│   ├── Views/                  # Razor Views
│   ├── Hubs/                   # SignalR ChatHub
│   ├── Services/               # FcmService (push notifications)
│   └── Resources/              # Localization .resx files (ru / az / en)
└── EcommersProject.API/        # Separate REST API with JWT auth & Scalar docs
```

---

## Getting Started

### Prerequisites
- .NET 10 SDK
- SQL Server (Azure SQL or LocalDB)

### Run

```bash
git clone https://github.com/natigea/FinallyProject.git
cd FinallyProject
dotnet run --project EcommersProject/EcommersProject.csproj
```

The app seeds the database with categories and sample data on first run.

### Optional: third-party services

| Service | Purpose | Config key |
|---------|---------|-----------|
| Stripe | Delivery & VIP payments | `Stripe:PublishableKey`, `Stripe:SecretKey` |
| Firebase FCM | Push notifications | `Firebase:ProjectId`, `Firebase:ServiceAccountPath` |
| Daily.co | Voice call rooms | `DailyCo:ApiKey` |
| SendGrid | Email delivery | `SendGrid:ApiKey`, `SendGrid:FromEmail` |

The app works without these keys — payment, call, and email features will be disabled or return errors.

### Default Admin Account

| Field | Value |
|-------|-------|
| Email | ahmedovnatig01@gmail.com |
| Password | Admin@123 |

---

## User Roles

| Role | Description |
|------|-------------|
| **Customer** | Browse, favorite, message sellers, order delivery |
| **Seller** | Post listings, receive orders, promote with VIP |
| **Admin** | Full access: moderation, user management, categories |

---

## License

Educational project — Code Academy Baku, 2026.
