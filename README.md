# 🍔 Foodo

A comprehensive **food delivery and e-commerce platform** built with a modern Clean Architecture approach.  
Foodo connects customers with merchants (food vendors) and facilitates seamless product browsing, ordering, and delivery management.

---

# 📌 Overview

Foodo is a full-stack food delivery application featuring:

- Multi-role authentication (Customer, Merchant, Driver)
- Merchant product management
- Customer ordering capabilities
- Driver coordination and delivery tracking
- JWT-based secure authentication
- Clean Architecture + CQRS implementation

---

# 🏗 Architecture

This project follows **Clean Architecture** principles with clear separation of concerns:

```
Foodo
│
├── Foodo.API
├── Foodo.Application
├── Foodo.Domain
└── Foodo.Infrastructure
```

---

## 🔹 Foodo.API
- RESTful API endpoints built with ASP.NET Core
- Request/response validation
- Global exception handling middleware
- CORS configuration
- Swagger/OpenAPI documentation
- JWT Authentication
- Rate limiting
- Request logging

---

## 🔹 Foodo.Application
- CQRS (Command Query Responsibility Segregation)
- MediatR handlers
- Application services
- DTOs for API contracts
- Business logic orchestration

---

## 🔹 Foodo.Domain
- Core business entities
- Domain models
- Enums (Customer, Merchant, Driver, Categories)
- Domain-driven design principles

---

## 🔹 Foodo.Infrastructure
- Entity Framework Core
- SQL Server integration
- Repository pattern
- Authentication & token management
- Caching & external integrations
- Database migrations

---

# 🚀 Key Features

## 👤 User Management
- Multi-role authentication (Customer / Merchant / Driver)
- JWT Authentication with refresh tokens
- Profile & address management

---

## 🛍 Product Management
- Merchant product creation & updates
- Rich product details
- Multiple product images
- 26+ food categories

---

## 📦 Order Management
- Order creation & checkout
- Order status tracking
- Driver assignment
- Merchant order dashboard

---

## ⚙ Technical Features
- Caching support
- Rate limiting
- Global exception handling
- Structured logging with Serilog
- Request validation filters
- CORS support

---

# 🛠 Technology Stack

- **Framework:** ASP.NET Core (.NET 8)
- **Database:** SQL Server
- **ORM:** Entity Framework Core
- **Authentication:** JWT + ASP.NET Core Identity
- **Architecture Pattern:** Clean Architecture + CQRS
- **Mediator:** MediatR
- **Logging:** Serilog
- **API Documentation:** Swagger/OpenAPI
- **Testing:** xUnit / NUnit

---

# ⚡ Getting Started

## ✅ Prerequisites

- .NET 8 SDK
- SQL Server (LocalDB or full SQL Server)
- Visual Studio / VS Code

---

## 📥 Installation

### 1️⃣ Clone the repository

```bash
git clone https://github.com/Taqey/Foodo.git
cd Foodo
```

---

### 2️⃣ Configure Database Connection

Update `appsettings.json`:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=YOUR_SERVER;Database=FoodoDb;Trusted_Connection=true;"
}
```

---

### 3️⃣ Apply Migrations

```bash
dotnet ef database update --project Foodo.Infrastructure
```

---

### 4️⃣ Run the Project

```bash
dotnet build
dotnet run --project Foodo.API
```

---

## 🌐 Access API

- Swagger UI:  
  `https://localhost:5001/swagger`

- API Base URL:  
  `https://localhost:5001/api`

---

# 📂 Project Structure Details

## 📌 CQRS Pattern

**Commands (Write Operations)**  
```
Foodo.Application/Commands/*
```

**Queries (Read Operations)**  
```
Foodo.Application/Queries/*
```

---

## 📌 Models & DTOs

```
Foodo.Application/Models/Dto/
Foodo.Domain/Entities/
```

Main Entities:
- TblCustomer
- TblMerchant
- TblDriver
- TblProduct
- TblOrder
- TblPhoto

---

## 📌 Database

```
Foodo.Infrastructure/Migrations/
Foodo.Infrastructure/Persistence/
```

---

# 🔌 API Endpoints

| Category | Endpoint |
|----------|----------|
| Authentication | `/api/auth/*` |
| Products | `/api/products/*` |
| Orders | `/api/orders/*` |
| Merchants | `/api/merchants/*` |
| Customers | `/api/customers/*` |
| Drivers | `/api/drivers/*` |

---

# 🗄 Database Tables

- AspNetUsers
- TblCustomers
- TblMerchants
- TblDrivers
- TblProducts
- TblOrders
- TblPhotos
- TblAddresses
- lkpAttribute
- lkpMeasureUnit

---

# ⚙ Configuration

## 📝 Logging (Serilog)

- Console Sink
- Seq (localhost:5341)
- File Logs → `logs/foodo_log.txt`

---

## 🔐 Authentication

- JWT tokens
- Refresh token support
- Role-based authorization

---

## 🌍 CORS Policy

Allowed origins:
- GitHub Pages
- Localhost
- Development file protocol

---

# 🧪 Testing

Run unit tests:

```bash
dotnet test
```

---

# 🤝 Contributing

Please ensure:

- Code follows Clean Architecture principles
- Unit tests are included
- Conventional commit messages are used
- All tests pass before PR

---

# 📜 License

MIT License

---

# 👨‍💻 Author

**Taqey Eldeen**  
GitHub: https://github.com/Taqey

---

# 📬 Support

If you encounter any issues, please open a GitHub Issue.

---

⭐ If you like this project, consider giving it a star!
