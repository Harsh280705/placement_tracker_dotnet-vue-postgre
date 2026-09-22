# Placement Tracker (Vue 3 + ASP.NET Core Web API + PostgreSQL)

Migrated from the previous ASP.NET Core MVC + Razor Views + SQLite implementation.
Business logic, model fields, validation rules, and CRUD behavior were reused;
only the presentation and data-access layers were replaced.

```text
Vue.js Frontend (Vue 3 + Vite)
        ↓ HTTP/JSON
ASP.NET Core Web API (API-only, no Razor)
        ↓
Entity Framework Core (Npgsql provider)
        ↓
PostgreSQL
```

## Technology Stack

- Backend: C# + ASP.NET Core 8 Web API, EF Core + Npgsql, Swagger/OpenAPI
- Frontend: Vue 3 + Vite + vue-router, `fetch`-based service layer
- Database: PostgreSQL
- Testing: xUnit + Moq (+ `Microsoft.AspNetCore.Mvc.Testing`, EF InMemory for isolated tests)

## Project Structure

```text
backend/
  Controllers/ApplicationsController.cs  # JSON API (converted from MVC controller)
  Models/Application.cs                  # entity + validation (reused)
  DTOs/ApplicationDtos.cs               # Create/Update/Response DTOs
  Data/ApplicationDbContext.cs          # EF Core -> PostgreSQL (was SQLite)
  Program.cs                             # AddControllers, Npgsql, CORS, Swagger
  appsettings.json                       # PostgreSQL connection string
  Properties/launchSettings.json         # http://localhost:5038
frontend/
  src/components/ApplicationForm.vue
  src/components/DeleteConfirmDialog.vue
  src/views/Dashboard.vue                # list all applications
  src/views/AddApplication.vue
  src/views/ApplicationDetails.vue
  src/views/EditApplication.vue
  src/services/applicationService.js     # get/create/update/delete helpers
  src/router/index.js
  src/App.vue
tests/                                   # xUnit + Moq (About 31 tests, isolated InMemory DB)
```

## Requirements

- .NET 8 SDK
- Node.js 18+ (npm)
- PostgreSQL 16 (running locally)

## PostgreSQL Setup

Create the database (adjust user/password to your environment):

```sql
CREATE DATABASE placement_tracker;
```

Configure the connection string via `backend/appsettings.json`
(`ConnectionStrings:Default`) or via environment variable (no hardcoded secrets
in code — the committed value is a local-dev default only):

```powershell
$env:ConnectionStrings__Default = "Host=127.0.0.1;Port=5432;Database=placement_tracker;Username=postgres;Password=<your-password>"
```

Tables are created automatically with `EnsureCreated()` on startup
(never destructive). In Development, 3 sample rows are seeded once when empty.

## How to Run the Backend

```powershell
dotnet run --project backend/PlacementTracker.Api.csproj
```

- API: http://localhost:5038
- Swagger: http://localhost:5038/swagger

## How to Run the Frontend

```powershell
cd frontend
npm install
npm run dev
```

- App: http://localhost:5173 (calls the API at http://localhost:5038; CORS is
  configured for exactly this origin)

## How to Run Tests

```powershell
dotnet test tests/PlacementTracker.Api.Tests.csproj
```

31 tests cover: GET all / GET one / GET 404, POST valid (201) / POST invalid
(400), PUT valid (200) / PUT 404, DELETE (204) / DELETE 404, all validation
rules, and a full HTTP CRUD round-trip. Tests use isolated InMemory databases
(a fresh DB per test/class) and never touch the real PostgreSQL database.

## API Endpoints

| Method | Route | Success | Not found |
|--------|-------|---------|-----------|
| GET | `/api/applications` | 200 + JSON array | — |
| GET | `/api/applications/{id}` | 200 + JSON object | 404 |
| POST | `/api/applications` | 201 + JSON object | 400 on invalid |
| PUT | `/api/applications/{id}` | 200 + JSON object | 404 (400 on invalid) |
| DELETE | `/api/applications/{id}` | 204 | 404 |

## Validation Rules (reused from the original implementation)

- company: required, 2–100 chars
- role: required, 2–100 chars
- status: required, one of Wishlist / Applied / Interview / Offer / Rejected
- applied_on: required unless status is Wishlist
- job_url: optional, must be a valid URL when supplied
- notes: optional, max 1000 chars
