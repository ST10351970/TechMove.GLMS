# Global Logistics Management System — Core Prototype

[![.NET Build and Test](https://github.com/ST10351970/TechMove.GLMS/actions/workflows/dotnet.yml/badge.svg)](https://github.com/ST10351970/TechMove.GLMS/actions/workflows/dotnet.yml)

ASP.NET Core MVC monolith implementing the Global Logistics Management System (GLMS)

**Module:** Enterprise Application Development — POE Part 2
**Stack:** ASP.NET Core MVC (.NET 8), Entity Framework Core, SQL Server LocalDB, xUnit + Moq + FluentAssertions

## Project Structure

| Project | Responsibility |
|---|---|
| `TechMove.GLMS.Web` | ASP.NET Core MVC presentation layer — Controllers, Razor views, dependency injection wiring |
| `TechMove.GLMS.Core` | Domain entities, EF Core DbContext (Fluent API), business services, design patterns |
| `TechMove.GLMS.Tests` | xUnit unit test suite (73 tests) covering business logic, validation, and integration points |

## Design Patterns Implemented (from Part 1 UML)

| Pattern | Location | Purpose |
|---|---|---|
| **Factory** | `Core/Services/Factories/ContractFactory.cs` | Encapsulates Contract creation with per-service-level defaults (Basic 6 mo, Premium 12 mo, Enterprise 24 mo) |
| **Strategy** | `Core/Services/Strategies/` | Interchangeable currency-to-ZAR conversion algorithms (USD, EUR, GBP) |
| **Observer** | `Core/Services/Observers/` | Broadcasts Contract status changes to audit logger and expired-contract guard |

## Key Features

- ✅ Three-tier separation: Web → Core (business logic) → Tests
- ✅ EF Core Fluent API configuration with normalised schema
- ✅ Async/await end-to-end with external Currency API integration
- ✅ Two-tier resilience cache (in-memory + disk fallback) survives app restarts
- ✅ Status state machine prevents illegal transitions (e.g. Expired → Active)
- ✅ Observer pattern enforces "no new requests on Expired/OnHold contracts"
- ✅ PDF-only file uploads with UUID naming and content-type validation
- ✅ Client-side and server-side validation
- ✅ 73 unit tests, automated via GitHub Actions on every push

## Running Locally

### Prerequisites
- .NET 8 SDK
- SQL Server LocalDB (bundled with Visual Studio)
- Optional: SQL Server Management Studio for database inspection

### Setup

```bash
# Restore packages
dotnet restore

# Apply migrations (creates TechMoveGLMS database in LocalDB)
dotnet ef database update --project TechMove.GLMS.Core --startup-project TechMove.GLMS.Web

# Run the web app
dotnet run --project TechMove.GLMS.Web
```

Open `https://localhost:7xxx` in your browser.

### Running Tests

```bash
dotnet test
```

## External Dependencies

- **ExchangeRate-API** (`https://open.er-api.com/`) — open access endpoint, no API key required, attribution provided in source.

## Submission Information

- **Student:** Lesego Letsapa
- **Student Number:** ST10351970