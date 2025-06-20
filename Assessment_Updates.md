# Crypto Price Tracker – Technical Assessment (Clean Architecture Rewrite)

*Last updated: ****19 Jun 2025***

---

## 1 · Challenge Overview

This repository contains my solution for the **.NET 6 Full‑Stack Developer Assessment** provided by **Envision Horizons**. The original brief required fixing and extending a Razor Pages + SQLite project that tracks cryptocurrency prices via the CoinGecko API.

### Key Deliverables & Acceptance Criteria

| ID     | Description                                                                                       |
| ------ | ------------------------------------------------------------------------------------------------- |
| **A1** | Project builds with **.NET 6 SDK 6.0.428**                                                        |
| **A2** | Price‑update logic (async, dedupe, persist) implemented in Clean Architecture handlers            |
| **A3** | **POST** `/api/crypto/update-prices` & **GET** `/api/crypto/latest-prices`                        |
| **A4** | Razor **Index.cshtml** shows Name, Symbol, Price, Currency, Icon, Last Updated (client TZ), Trend |
| **A5** | `IconUrl` stored in DB                                                                            |
| **A6** | Clear UI feedback (spinner → success / failure badge)                                             |
| **A7** | ≥ 2 unit tests validating handler logic                                                           |
| **A8** | No DB file / EF migrations committed                                                              |
| **A9** | README documents assumptions, structure & decisions                                               |

All criteria above are ✅ met (see **section 8** Checklist).

---

## 2 · Architecture Rationale

| Layer              | Project                             | Key Responsibility                                                                                                           |
| ------------------ | ----------------------------------- | ---------------------------------------------------------------------------------------------------------------------------- |
| **Presentation**   | `CryptoPriceTracker.Api`            | ASP.NET Core MVC + Razor; thin controllers using MediatR; serves static assets from *wwwroot*.                               |
| **Application**    | `CryptoPriceTracker.Application`    | CQRS/mediator handlers (`UpdatePrices`, `GetLatestPrices`); validation; domain ports (`ICryptoRepository`, `IPriceFetcher`). |
| **Domain**         | `CryptoPriceTracker.Domain`         | Pure entities `CryptoAsset`, `CryptoPriceHistory`; value objects & rules.                                                    |
| **Infrastructure** | `CryptoPriceTracker.Infrastructure` | EF Core SQLite persistence, `CoinGeckoHttpClient` with Polly retry, repository implementation.                               |

> **Why Clean Architecture?**  Loose coupling enables easier testing, future DB/API swaps, and clear reviewer navigation.

---

## 3 · Version Matrix & Tooling

| Component            | Version                                       | Rationale                                         |
| -------------------- | --------------------------------------------- | ------------------------------------------------- |
| **.NET SDK**         | **6.0.428**                                   | Matches assessment; LTS supported until Nov 2025. |
| **EF Core / SQLite** | 6.0.25                                        | Same major as runtime; stable provider.           |
| **MediatR**          | 10.0.1                                        | Lightweight CQRS mediator.                        |
| **FluentValidation** | 11.8.x                                        | Declarative rules (future work).                  |
| **Polly**            | 7.2.x + `Microsoft.Extensions.Http.Polly` 6.0 | Transient HTTP retry (2 → 4 → 8 s).               |
| **Swashbuckle**      | 6.5.x                                         | Swagger UI for reviewer.                          |
| **xUnit**            | 2.6.x                                         | Unit test framework.                              |
| **Moq**              | 4.18.x                                        | Mock ports in tests.                              |
| **Luxon**            | 3.x (CDN)                                     | Client‑side TZ conversion.                        |

---

## 4 · Structural Changes

| Change                                                                       | Motivation                                                                       |
| ---------------------------------------------------------------------------- | -------------------------------------------------------------------------------- |
| Split monolith into **Domain / Application / Infrastructure / Api** projects | Enforces layer boundaries, improves clarity.                                     |
| Removed `CryptoPriceService.cs`                                              | Replaced by **UpdatePricesHandler** (write) & **GetLatestPricesHandler** (read). |
| Introduced **ports & adapters** (`ICryptoRepository`, `IPriceFetcher`)       | Decouple Application from EF Core & HTTP.                                        |
| Added **Polly** retry to CoinGecko client                                    | Handles 429/5xx gracefully.                                                      |
| Enforced **UNIQUE** index (Asset + Date)                                     | Ensures deduplication at DB level.                                               |
| Implemented **Razor UI** with vanilla JS + Luxon                             | Displays data, spinner, trend arrows.                                            |
| Added **wwwroot/css/site.css**                                               | Central static asset location.                                                   |
| `.gitignore` extended                                                        | Excludes `*.db`, `**/Migrations/`, `bin/`, `obj/`.                               |

---

## 5 · Build & Run

```bash
# Restore & build
> dotnet build

# (Re)create local SQLite file
> dotnet ef database update \
      --project src/CryptoPriceTracker.Infrastructure \
      --startup-project src/CryptoPriceTracker.Api

# Launch
> dotnet run --project src/CryptoPriceTracker.Api
# Browser
→ https://localhost:7207/Home/Index  (UI)
→ https://localhost:7207/swagger      (API docs)
```

---

## 6 · Test Suite

```bash
> dotnet test
# ➜  Test Run Successful. Total tests: 2  (handlers coverage)
```

- `GetLatestPricesHandlerTests` – returns neutral price when no history.
- `UpdatePricesHandlerTests` – ignores duplicate timestamp, repo not invoked.

---

## 7 · Usage Walk‑Through

1. **Open UI** → Click **⟳ Update Prices**.
2. Spinner appears, backend POST fetches CoinGecko, inserts deduped rows.
3. Table fades‑in with fresh prices, icon, local time, arrow trend. Subsequent clicks show `Updated (0)` when no new data.
4. Swagger allows manual API testing.

---

## 8 · Deliverables Checklist

| ID     | Status                                          |
| ------ | ----------------------------------------------- |
| **A1** | ✅ .NET 6.0.428 build green                      |
| **A2** | ✅ Update logic fixed, async & dedupe            |
| **A3** | ✅ POST / GET endpoints operational & documented |
| **A4** | ✅ Razor UI shows all required columns, Luxon TZ |
| **A5** | ✅ `IconUrl` saved on first fetch                |
| **A6** | ✅ Spinner + success/failure badge               |
| **A7** | ✅ 2 unit tests pass (`dotnet test`)             |
| **A8** | ✅ `.gitignore` excludes DB & migrations         |
| **A9** | ✅ README details structure & assumptions        |

---

## 9 · Assumptions & Decisions

- **Two seed assets** (Bitcoin, Ethereum) configured in `OnModelCreating` for predictable first run.
- **Price trend** compares latest vs. previous record; if only one record ➞ neutral arrow.
- **Currency fixed to USD** as CoinGecko default; easily extendable.
- **Luxon** chosen over native `Intl` for consistent TZ formatting across browsers.
- Skipped advanced validation (e.g. rate‑limit back‑off) to stay within 5‑day scope.

---

## 10 · Changelog (abridged)

| Date        | Note                                             |
| ----------- | ------------------------------------------------ |
| 18‑Jun‑2025 | Initial Clean Architecture skeleton & DI wiring. |
| 18‑Jun‑2025 | Added CoinGecko client with Polly retry.         |
| 19‑Jun‑2025 | Razor UI, spinner, tests & README finalized.     |

---

*Prepared by ****German Rivera**** – solution branch: **`solution/german-rivera`**.*