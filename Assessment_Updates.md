# Crypto Price Tracker – Technical Assessment (Clean Architecture Rewrite)

*Last updated: ****18 Jun 2025***

---

## 1 · Challenge Overview

This repository contains my solution for the **.NET 6 Full‑Stack Developer Assessment** provided by Envision Horizons. The original brief required fixing and extending a Razor Pages/SQLite project that tracks cryptocurrency prices from the CoinGecko API.

Key deliverables

|  ID  | Acceptance Criterion                                                                            |
| ---- | ----------------------------------------------------------------------------------------------- |
|  A1  | Project builds on **.NET 6 SDK 6.0.428**                                                        |
|  A2  | `CryptoPriceService` logic fixed (fetch, async, dedupe, persist)                                |
|  A3  | API endpoints:  • POST `/api/crypto/update-prices`  • GET `/api/crypto/latest-prices`           |
|  A4  | Razor `Index.cshtml` shows Name, Symbol, Price, Currency, Icon, Last Updated (client TZ), Trend |
|  A5  | `IconUrl` stored in DB                                                                          |
|  A6  | Clear UI feedback after update attempt                                                          |
|  A7  | ≥1 unit test validating logic                                                                   |
|  A8  | No DB file / migrations committed                                                               |
|  A9  | README documents any structural changes                                                         |

---

## 2 · Architecture Rationale

|  Layer             | Project                             | Responsibility                                                                             |
| ------------------ | ----------------------------------- | ------------------------------------------------------------------------------------------ |
| **Presentation**   | `CryptoPriceTracker.Api`            | ASP.NET Core MVC + Razor. Thin controllers using MediatR.                                  |
| **Application**    | `CryptoPriceTracker.Application`    | CQRS handlers (`UpdatePrices`, `GetLatestPrices`), validators, interfaces (ports).         |
| **Domain**         | `CryptoPriceTracker.Domain`         | Entities `CryptoAsset`, `CryptoPrice`, value objects, domain rules.                        |
| **Infrastructure** | `CryptoPriceTracker.Infrastructure` | EF Core SQLite persistence, `CoinGeckoHttpClient`, Polly retry, repository implementation. |

We selected this **Clean Architecture** approach for:

- **Scalability** – layers are independent; swapping SQLite→MySQL requires only Infrastructure changes.
- **Testability** – Application layer can be unit‑tested without DB or HTTP.
- **Reviewer clarity** – folders follow a predictable pattern; original endpoints and Razor view remain unchanged.

---

## 3 · Version Matrix & Tooling

|  Component               | Version                                       | Reason                                                                    |
| ------------------------ | --------------------------------------------- | ------------------------------------------------------------------------- |
|  .NET SDK                | **6.0.428** (LTS)                             | Matches assessment requirement; receives security patches until Nov 2025. |
|  EF Core                 | 6.0.25                                        | Same major as runtime; SQLite provider mature.                            |
|  MediatR                 | 10.0.1                                        | Latest 10.x compatible with .NET 6; lightweight CQRS.                     |
|  FluentValidation        | 11.8.x                                        | Declarative validation; integrates with DI.                               |
|  Polly                   | 7.2.x + `Microsoft.Extensions.Http.Polly` 6.0 | Transient HTTP retries for CoinGecko client.                              |
|  Swagger (`Swashbuckle`) | 6.5.x                                         | API discoverability during review.                                        |
|  xUnit                   | 2.6.x                                         | Unit testing framework.                                                   |

Packages are locked to **major 6** where framework compatibility matters. A `global.json` pins the SDK to avoid accidental builds on .NET 8 or other .NET versions.

---

## 4 · Structural Changes

- **Removed** legacy single‑project layout.
- **Added** solution folders and four separate `.csproj` files (Domain, Application, Infrastructure, Api).
- **Migrated** entities from `Models/` to `Domain/Entities`.
- **Replaced** `CryptoPriceService` with `UpdatePricesHandler` (CQRS).
- **Introduced** `ICryptoRepository` & `IPriceFetcher` ports to decouple persistence & HTTP.
- **Implemented** DI extensions: `AddApplication()` and `AddInfrastructure()` for clean `Program.cs`.
- **Added** Polly retry policy on HTTP client (exponential back‑off 2 s → 4 s → 8 s).
- **Added** `.gitignore` rules for `*.db`, `**/Migrations/`.

> *Commit history documents each refactor step; see branch **`solution/german-rivera`**.*

---

## 5 · Dependency Graph

```text
Api ──► Application ──► Domain
   │            │
   └────────► Infrastructure ──► Domain
```

No layer references a more external one.

---

## 6 · Build & Run

```bash
# Restore & build
> dotnet build

# Create / update database (local)
> dotnet ef database update --project src/CryptoPriceTracker.Infrastructure \
      --startup-project src/CryptoPriceTracker.Api

# Launch application
> dotnet run --project src/CryptoPriceTracker.Api
# Navigate to http://localhost:5000
```

---

## 7 · Test Strategy

- **Unit**: Application handlers tested with mocked ports (Moq).
- **Integration**: Infrastructure tested using SQLite in‑memory & Respawn to reset state.
- GitHub Actions workflow runs `dotnet test` on every push.

---

## 8 · Outstanding TODO

-

---

## 9 · Changelog

|  Date           | Change                                                                                        |
| --------------- | --------------------------------------------------------------------------------------------- |
| **18 Jun 2025** | Initial Clean Architecture skeleton compiled; DI, EF, Polly, MediatR, FluentValidation wired. |

---

