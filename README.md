# 🏛️ Atlantis Portfolio Backend

![.NET](https://img.shields.io/badge/.NET-10.0-512BD4?style=flat&logo=dotnet)
![C#](https://img.shields.io/badge/C%23-13.0-239120?style=flat&logo=c-sharp)
![Tests](https://img.shields.io/badge/tests-9%2F9%20passing-brightgreen?style=flat)

A production-grade **ASP.NET Core 10 Web API** that powers a personal portfolio management system. Provides secure authentication, comprehensive CRUD operations, audit logging, and pagination — following a clean **3-tier architecture**.

---

## ✨ Features

- 🔐 **JWT Authentication** with access & refresh tokens
- 🔒 **BCrypt Password Hashing**
- 🛡️ **Account Lockout** after failed login attempts
- 🔑 **Master Password** required for critical actions (e.g., user deletion)
- 📊 **Audit Logging** for all critical actions
- 🚦 **Rate Limiting** on authentication endpoints
- 📄 **Pagination** for large data sets
- 🧪 **Unit Tests** with xUnit, Moq & SQLite In-Memory
- 📘 **Swagger / OpenAPI** with Bearer auth
- ⚠️ **Global Exception Middleware** with traceId tracking

---

## 🏗️ Architecture

The project follows a strict **3-tier architecture**:

```

API (Atlantis Portfolio-Backend)
↓
BLL (Atlantis Portfolio-Backend-BLL)
↓
DAL (Atlantis Portfolio-Backend-DAL)

```

| Layer | Responsibility |
|---|---|
| **API** | Controllers, DTOs, Middleware, DI |
| **BLL** | Business rules, validation, services |
| **DAL** | Entities, EF Core, database access |

---

## 🛠️ Tech Stack

| Category | Technology |
|---|---|
| Framework | .NET 10.0 / ASP.NET Core |
| Language | C# 13 |
| ORM | Entity Framework Core 10 |
| Database | SQL Server |
| Auth | JWT Bearer Tokens |
| Password Hashing | BCrypt.Net-Next |
| API Docs | Swagger / Swashbuckle |
| Testing | xUnit · Moq · FluentAssertions · SQLite In-Memory |
| Rate Limiting | ASP.NET Core Rate Limiter |

---

## 🚀 Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download)
- SQL Server (LocalDB or full instance)
- Visual Studio 2022+ or VS Code

### Installation

**1. Clone the repository**

```bash
git clone https://github.com/Abdallah-Hameed/Atlantis-Portfolio-Backend.git
cd "Atlantis Portfolio-Backend"
```

2. Configure secrets (User Secrets)

From the API project folder:

```bash
cd "Atlantis Portfolio-Backend"
dotnet user-secrets init
dotnet user-secrets set "AtlantisPortfolio:Jwt:Key" "YOUR_SECRET_KEY_MIN_32_CHARS"
dotnet user-secrets set "AtlantisPortfolio:Jwt:Issuer" "AtlantisPortfolioBackend"
dotnet user-secrets set "AtlantisPortfolio:Jwt:Audience" "AtlantisPortfolioClient"
dotnet user-secrets set "AtlantisPortfolio:ConnectionStrings:DefaultConnection" "Server=localhost;Database=AtlantisPortfolioDB;User Id=sa;Password=YOUR_PASSWORD;TrustServerCertificate=True;"
```

3. Apply database migrations

```bash
dotnet ef database update --project "Atlantis Portfolio-Backend-DAL" --startup-project "Atlantis Portfolio-Backend"
```

4. Run the API

```bash
dotnet run --project "Atlantis Portfolio-Backend"
```

5. Open Swagger

Navigate to: https://localhost:7072/swagger

⚠️ First time: You may see a certificate warning. Click Advanced → Proceed to localhost.

---

🌐 API Base URL

Environment URL
Development (HTTPS) https://localhost:7072
Development (HTTP) http://localhost:5232

---

🔐 Authentication Flow

All protected endpoints require the header:

```
Authorization: Bearer <accessToken>
```

Steps:

1. Login → POST /api/Auth/Login with { email, password }
2. Receive { accessToken, refreshToken }
3. Use accessToken in the Authorization header
4. When accessToken expires (15 min), call POST /api/Auth/Refresh with { refreshToken }
5. To log out, call POST /api/Auth/Logout with { refreshToken }

In Swagger:

1. Call POST /api/Auth/Login.
2. Copy the accessToken from the response.
3. Click the Authorize button (top-right).
4. Enter: Bearer <accessToken> and click Authorize.

---

📊 Main Endpoints

Method Endpoint Description Auth
POST /api/Auth/Login Authenticate user ❌
POST /api/Auth/Refresh Refresh access token ❌
POST /api/Auth/Logout Revoke refresh token ✅
GET /api/Project List projects ❌
POST /api/Project Create project ✅
GET /api/Skill List skills ❌
GET /api/PortfolioProfile Get profile info ❌
GET /api/AuditLog?pageNumber=1&pageSize=20 Paginated audit logs ✅
PUT /api/User/Update/Password Change own password ✅
DELETE /api/User/Delete Delete user (requires master password) ✅

Full API documentation: Swagger UI at /swagger.

---

🧪 Running Tests

```bash
cd "Atlantis-Portfolio-Backend-Tests"
dotnet test
```

Expected output:

```
Passed! - Failed: 0, Passed: 9, Skipped: 0, Total: 9
```

---

🐛 Common Issues

Issue Solution
CORS error from frontend Frontend must run on http://localhost:3000. To change, edit Program.cs CORS policy.
Certificate error in Swagger Accept the warning: Advanced → Proceed to localhost.
401 Unauthorized Token expired or invalid. Call /api/Auth/Refresh.
SQL Server connection error Verify connection string. Ensure SQL Server is running.
Migrations not found Ensure you're in the right project folder with --project flag.

---

📂 Project Structure

```
Atlantis Portfolio-Backend/
├── Atlantis Portfolio-Backend/          # API Layer
│   ├── Controllers/
│   ├── DTOs/
│   ├── Middleware/
│   ├── Models/
│   └── Program.cs
│
├── Atlantis Portfolio-Backend-BLL/      # Business Logic Layer
│   ├── Enums/
│   ├── Models/
│   └── Services/
│
├── Atlantis Portfolio-Backend-DAL/      # Data Access Layer
│   ├── Context/
│   └── Entities/
│
└── Atlantis-Portfolio-Backend-Tests/    # xUnit Tests
    └── clsAuthServiceTests.cs
```

---

🗺️ Roadmap

☑ 3-tier architecture
☑ JWT authentication + refresh tokens
☑ BCrypt password hashing
☑ Account lockout mechanism
☑ Master password for critical actions
☑ Audit logging
☑ Pagination
☑ Unit tests (9/9)
☑ Global exception middleware
☐ Frontend integration
☐ Deployment

---

👨‍💻 Author

Abdallah Hameed — Junior Backend Developer | .NET
🔗 GitHub

---

📄 License

This project is licensed under the MIT License.
