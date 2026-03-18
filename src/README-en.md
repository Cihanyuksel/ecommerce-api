# E-Commerce Microservices API 

A backend application built with modern software architecture principles (Onion Architecture, CQRS, Event-Driven) and the 12-Factor App methodology. Designed as a **Case Study** for backend developer interview processes.

---

## Architecture & Technologies

The system is composed of independent microservices, each with its own responsibility, following modern enterprise standards:

- **.NET 8 Web API:** Core framework.
- **Onion (Clean) Architecture:** Domain, Application, Infrastructure, and API layers — isolated and testable.
- **CQRS Pattern:** Command and Query responsibilities separated via the `MediatR` library.
- **Event-Driven Architecture:** Asynchronous, event-driven communication between services using `RabbitMQ` and `MassTransit`.
- **API Gateway:** Centralized routing and Rate Limiting via `YARP (Yet Another Reverse Proxy)`.
- **Centralized Logging:** Asynchronous, structured logging infrastructure using `Serilog` and `Seq`.
- **Caching:** Listing operations optimized with `Redis Distributed Cache`.
- **Cache Invalidation (Redis):** Product listings are served from Redis to maximize read performance (CQRS - Query). To ensure data consistency, the relevant cache key (`all_products`) is immediately invalidated on any create, update, or delete operation (CQRS - Command), preventing users from seeing stale data.
- **Authentication:** Secure authorization with Refresh Token support using `ASP.NET Core Identity` and `JWT`.

---

## Services & Ports

| Service | Port | Description |
| :--- | :--- | :--- |
| **Gateway.API** | `5123` | Single entry point for all services; handles Rate Limiting. |
| **Auth.API** | `5259` | User registration, login, JWT generation, and Refresh Token management. |
| **Product.API** | `5127` | Product create/update/delete and Redis Cache-backed listing. |
| **Log.API** | `5295` | Background Worker that consumes RabbitMQ messages and writes logs to Seq via Serilog. |

---

## Project Structure

```
ecommerce-api/
├── docker-compose.yml
└── src/
    ├── Services/
    |   ├── Gateway.API/
    │   ├── Auth/
    │   │   ├── Auth.API/
    │   │   ├── Auth.Application/
    │   │   ├── Auth.Domain/
    │   │   └── Auth.Infrastructure/
    │   └── Product/
    │       ├── Product.API/
    │       ├── Product.Application/
    │       ├── Product.Domain/
    │       └── Product.Infrastructure/
    |       ├── Log.API/
    └── Shared/
```

---

## Getting Started

> **⚠️ Note (Zero Friction Approach):**
> To make reviewing and testing as frictionless as possible, database passwords, RabbitMQ credentials, and JWT keys have been intentionally included in the `appsettings.json` files. **No `.env` configuration is required.**

### Prerequisites

Make sure the following tools are installed on your machine:

- [Docker Desktop](https://www.docker.com/products/docker-desktop/) (v24+)
- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [EF Core CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet): `dotnet tool install --global dotnet-ef`

---

### 1. Clone the Repository

```bash
git clone https://github.com/Cihanyuksel/ecommerce-api.git
cd ecommerce-api
```

### 2. Start the Infrastructure (Docker Compose)

Starts SQL Server, RabbitMQ, Redis, and Seq:

```bash
docker-compose up -d
```

To verify all containers are running:

```bash
docker-compose ps
```

### 3. Apply Database Migrations

**Auth Service:**
```bash
dotnet ef database update \
  --project src/Services/Auth/Auth.Infrastructure \
  --startup-project src/Services/Auth/Auth.API
```

> Once the Auth database is migrated, an admin user (`cihan@admin.com` / `M12*117go`) will be seeded automatically.

**Product Service:**
```bash
dotnet ef database update \
  --project src/Services/Product/Product.Infrastructure \
  --startup-project src/Services/Product/Product.API
```

### 4. Run the Services

**Via IDE (Visual Studio / Rider):**

Go to Solution Properties → Multiple Startup Projects and set the following projects to `Start`:

- `Gateway.API`
- `Auth.API`
- `Product.API`
- `Log.API`

**Run via Terminal**

```bash
dotnet run --project src/Services/Gateway.API
dotnet run --project src/Services/Auth/Auth.API
dotnet run --project src/Services/Product/Product.API
dotnet run --project src/Services/Log.API
```

---

## End-to-End Test Scenario

### 1. Log In and Get a Token

```
POST http://localhost:5123/api/auth/login
Content-Type: application/json

{
  "email": "cihan@admin.com",
  "password": "M12*117go"
}
```

Copy the returned `accessToken`.

### 2. Create a Product

```
POST http://localhost:5123/api/products
Authorization: Bearer <accessToken>
Content-Type: application/json

{
  "name": "Test Product",
  "price": 199.99,
  "stock": 50
}
```

### 3. Verify the Logs

When a product is successfully created, `Product.API` publishes an event to RabbitMQ. `Log.API` consumes it and writes the log entry to Seq.

Open the Seq UI at **http://localhost:5341** (password: `M12*117go!`) to see the incoming log record.

---

## Management Panels

| Service | URL | Credentials |
| :--- | :--- | :--- |
| **Seq (Logging)** | http://localhost:5341 | `admin` / `M12*117go` |
| **RabbitMQ Management** | http://localhost:15672 | `guest` / `guest` |
| **RedisInsight** | http://localhost:8001 | No password |
| **Swagger – Auth** | http://localhost:5259/swagger | — |
| **Swagger – Product** | http://localhost:5127/swagger | — |

---