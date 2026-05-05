# 🛍️ Swoop Marketplace

**Swoop** is a full-stack second-hand marketplace web application where users can buy and sell used items. Sellers can post listings with images, buyers can browse and filter by category, save favourites, and contact sellers via real-time chat — all within a clean, role-based platform.

---

## 📋 Table of Contents

- [Features](#features)
- [Tech Stack](#tech-stack)
- [Project Structure](#project-structure)
- [Prerequisites](#prerequisites)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Setup](#database-setup)
- [Running the Application](#running-the-application)
- [API Documentation](#api-documentation)
- [User Roles](#user-roles)
- [Testing](#testing)

---

## ✨ Features

- **Listings** – Create, edit, delete and browse second-hand item listings with title, description, price, condition, location and multiple images
- **Categories** – Filter listings by category
- **Real-time messaging** – Chat with sellers directly on a listing page using SignalR WebSockets
- **Bookmarks** – Save listings for later
- **Reports** – Flag inappropriate listings
- **User profiles** – Editable profile with avatar, bio and contact info
- **Admin panel** – Manage users, listings and reports
- **JWT authentication** – Secure token-based auth between the frontend and backend API
- **Soft delete** – Deleted listings are hidden rather than permanently removed
- **View tracking** – Listing view counts are tracked per user

---

## 🛠️ Tech Stack

| Layer | Technology |
|---|---|
| Backend API | ASP.NET Core 8 Web API |
| Frontend | ASP.NET Core 8 Razor Pages |
| Real-time | ASP.NET Core SignalR |
| ORM | Entity Framework Core 8 |
| Database | MySQL (via Pomelo EF Core provider) |
| Authentication | ASP.NET Core Identity + JWT Bearer |
| API Docs | Swagger / Swashbuckle |
| Testing | MSTest |

---

## 📁 Project Structure

```
SwoopMarketplaceProject/
├── SwoopMarketplaceProject/          # Shared models, DbContext, Migrations
│   ├── Models/
│   └── Migrations/
├── SwoopMarketplaceProjectBackendAPI/ # REST API + SignalR hub
│   ├── Controllers/
│   ├── Data/                          # Seeders (users, categories, listings, images)
│   ├── Hubs/                          # ChatHub (SignalR)
│   └── Program.cs
├── SwoopMarketplaceProjectFrontend/   # Razor Pages UI
│   ├── Pages/
│   │   ├── Account/                   # Login, Register, Logout
│   │   ├── Admin/                     # Admin dashboard
│   │   ├── Listings/                  # Browse, Details, Create, Edit, Saved
│   │   ├── Messages/                  # Inbox, Conversation
│   │   ├── Reports/                   # Report a listing
│   │   └── Users/                     # My Profile
│   ├── Services/                      # API client services (typed HttpClient wrappers)
│   └── Program.cs
└── SwoopMarketplaceProjectTests/      # Unit & integration tests
```

---

## ✅ Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- MySQL Server (8.x recommended)
- A MySQL client (e.g. MySQL Workbench or DBeaver)

---

## 🚀 Getting Started

### 1. Clone the repository

```bash
git clone https://github.com/<your-username>/SwoopMarketplaceProject.git
cd SwoopMarketplaceProject
```

### 2. Restore NuGet packages

```bash
dotnet restore
```

---

## ⚙️ Configuration

### Backend API – `SwoopMarketplaceProjectBackendAPI/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "server=localhost;database=swoop;user=root;password=YOUR_PASSWORD;",
    "IdentityConnection": "server=localhost;database=swoopidentity;user=root;password=YOUR_PASSWORD;"
  },
  "Jwt": {
    "Key": "YOUR_SECRET_KEY_AT_LEAST_32_CHARS",
    "Issuer": "SwoopBackendAPI",
    "Audience": "SwoopClients"
  }
}
```

### Frontend – `SwoopMarketplaceProjectFrontend/appsettings.json`

```json
{
  "Api": {
    "BaseUrl": "https://localhost:7001/"
  }
}
```

> ⚠️ **Never commit real credentials or JWT secrets to source control.** Use `appsettings.Development.json` or environment variables for local development.

---

## 🗄️ Database Setup

The project uses two separate MySQL databases:

| Database | Purpose |
|---|---|
| `swoop` | Application data (listings, users, messages, etc.) |
| `swoopidentity` | ASP.NET Core Identity (authentication) |

Apply migrations for both contexts:

```bash
# Application database
dotnet ef database update --project SwoopMarketplaceProject --startup-project SwoopMarketplaceProjectBackendAPI --context SwoopContext

# Identity database
dotnet ef database update --project SwoopMarketplaceProjectBackendAPI --context ApplicationDbContext
```

Seed data (demo users, categories, listings and images) is applied automatically on first run.

---

## ▶️ Running the Application

Both projects must run simultaneously. Open two terminals:

**Terminal 1 – Backend API**
```bash
cd SwoopMarketplaceProjectBackendAPI
dotnet run
```
The API starts at `https://localhost:7001` (or as configured in `launchSettings.json`).

**Terminal 2 – Frontend**
```bash
cd SwoopMarketplaceProjectFrontend
dotnet run
```
The frontend starts at `https://localhost:7127`.

---

## 📖 API Documentation

Swagger UI is available in development mode at:

```
https://localhost:7001/swagger
```

All protected endpoints require a `Bearer` token in the `Authorization` header. You can obtain a token by calling `POST /api/Auth/login`.

---

## 👥 User Roles

| Role | Permissions |
|---|---|
| `User` | Browse listings, create listings, message sellers, bookmark, report |
| `Admin` | All User permissions + manage reports and moderate listings |
| `Owner` / `Tulaj` | Full access including user management |

Demo accounts are seeded automatically on first startup (see `IdentitySeeder.cs` for credentials).

---

## 🧪 Testing

The test project (`SwoopMarketplaceProjectTests`) contains:

- **Backend controller attribute tests** – Verify that controllers have the correct authorization attributes
- **Backend integration tests** – Test API endpoint behaviour
- **Frontend unit tests** – Test helper utilities such as `PriceExtensions`

Run all tests:

```bash
dotnet test
```
