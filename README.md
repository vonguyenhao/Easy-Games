# Easy Games — Project Brief

A minimal ASP.NET Core MVC demo shop for books, games, and toys. It showcases clean MVC structure, role-based access (Owner/Customer), and a reliable checkout flow with server-side validation.

---

## Overview
**Stack:** ASP.NET Core MVC (.NET 8), EF Core (SQL Server LocalDB), ASP.NET Identity  
**Roles:** `Owner`, `Customer` (seeded demo users)

**Core Features**
- Public **Catalog** with category filter + keyword search; currency-formatted prices.
- **Products CRUD** (Owner-only); `CreatedAt` set on server, hidden from customers.
- **Cart & Checkout** (Session-backed); validates stock; creates `Order` + `OrderItem`, decrements stock.
- **Users** (Owner): list/create/delete customers; registrations auto-assign `Customer`.
- **Orders history:** Customer sees own orders; Owner sees all.
- **403/404** pages; migrations + seed run on startup.

---

## User Stories

### Kevin Vo — Founder of Easy Games Ltd
*As an Owner, I manage products and users and review all orders so the store runs correctly and securely.*

### Hao Vo — Customer purchases items
*As a Customer, I browse the catalog, manage a cart, checkout, and view my order history so I can buy easily.*

---

## How to Run

**Prerequisites**
- Visual Studio 2022 (or `dotnet` CLI)
- .NET 8 SDK
- SQL Server LocalDB (bundled with Visual Studio)

**Steps**
1. Clone repo and open `EasyGames.sln`.
2. (If needed) Package Manager Console → `Update-Database`
3. Run (**F5**).

**Steps (CLI)**
```bash
dotnet restore
dotnet ef database update
dotnet run --project EasyGames.Web

**Seed Accounts**

1. Owner: admin@easygames.com / Easygames1@

2. Customer: customer@easygames.com / Easygames1@