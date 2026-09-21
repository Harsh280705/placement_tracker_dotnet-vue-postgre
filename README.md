# Placement Tracker (.NET)

Browser-based CRUD web app for tracking job and internship applications.

## Purpose

- Create applications
- View all applications (dashboard)
- View one application
- Edit applications
- Delete applications (POST + confirmation)

Data is stored persistently in SQLite via Entity Framework Core.

## Technology Stack

- C#, .NET 8
- ASP.NET Core MVC, Razor Views
- Entity Framework Core + SQLite
- HTML + plain CSS (no Bootstrap/Tailwind/React)
- NuGet
- xUnit + Moq + Microsoft.AspNetCore.Mvc.Testing (tests)

## Requirements

- .NET 8 SDK (`dotnet --version` should show 8.x)
- No other services needed (SQLite file is created automatically)

## How to Run

```powershell
dotnet restore
dotnet run
```

Then open the URL shown in the terminal, usually:

- http://localhost:5000
- or http://localhost:5xxx (check terminal output)

Dashboard = `GET /`.

Stop with `Ctrl+C`. Data survives restart because it lives in `Data/placement_tracker.db`.

### Ports

The app uses standard ASP.NET Core Kestrel ports from `Properties/launchSettings.json`
(`http://localhost:5000` by default when run with `dotnet run`).
Check the terminal output for the exact URL.

## How to Build

```powershell
dotnet build
```

## How to Run Tests

```powershell
dotnet test PlacementTracker.Tests/PlacementTracker.Tests.csproj
```

Tests use isolated InMemory databases (a fresh DB per test) plus
`WebApplicationFactory` with `Environment="Testing"`.
They never modify `Data/placement_tracker.db`.

25 tests cover: dashboard, valid/invalid create, details, 404,
edit page, update, delete, validation edge cases, empty DB,
and HTTP integration.

## Database Information

- File: `Data/placement_tracker.db` (SQLite, persistent)
- Created automatically with `EnsureCreated()` (no destructive recreate)
- In Development, 3 sample rows are seeded once when the table is empty:
  Acme Labs / Python Intern / Applied / 16 Sep 2026,
  Northstar / Graduate Engineer / Interview / 12 Sep 2026,
  Contoso / Backend Intern / Rejected / 05 Sep 2026
- Tests use InMemory only.

## Project Structure

```text
PlacementTracker.csproj
Program.cs                      # MVC, EF SQLite, routing, seed, Testing guard
Controllers/
  ApplicationsController.cs     # all CRUD routes
  ErrorController.cs            # /Error/{code} -> custom 404
Models/
  Application.cs                # entity + DataAnnotations + IValidatableObject
Data/
  ApplicationDbContext.cs       # DbContext + DbSet<Application>
  placement_tracker.db          # created at runtime (SQLite)
Views/
  Applications/Index.cshtml     # dashboard (GET /)
  Applications/Create.cshtml    # GET /applications/new
  Applications/Details.cshtml   # GET /applications/{id}
  Applications/Edit.cshtml      # GET /applications/{id}/edit
  Shared/_Layout.cshtml         # shared layout (nav + CSS)
  Shared/404.cshtml             # custom not-found page
wwwroot/css/site.css            # plain responsive CSS
PlacementTracker.Tests/         # xUnit + Moq, isolated DB
README.md
```

## Main Routes

| Method | Route | Action |
|--------|-------|--------|
| GET | `/` | Dashboard (all applications) |
| GET | `/applications/new` | Create form |
| POST | `/applications` | Create (validate, save, redirect to details) |
| GET | `/applications/{id}` | Details (every field, or custom 404) |
| GET | `/applications/{id}/edit` | Pre-filled edit form |
| POST | `/applications/{id}/edit` | Update (validate, save, redirect) |
| POST | `/applications/{id}/delete` | Delete (POST only, confirm, redirect) |

Post/Redirect/Get is used after every successful POST.

## CRUD Behavior

- Query: `OrderByDescending(Id)`, `FindAsync(id)`, LINQ
- Add: `DbSet.Add()` + `SaveChangesAsync()`
- Update: load entity, set fields, `SaveChangesAsync()`
- Delete: `DbSet.Remove()` + `SaveChangesAsync()` (POST only, JS `confirm()`)
- GET never deletes data.

## Validation Rules

- company: required, 2-100 chars
- role: required, 2-100 chars
- status: required, one of Wishlist / Applied / Interview / Offer / Rejected
- applied_on: required unless status is Wishlist
- job_url: optional, must be valid URL if provided
- notes: optional, max 1000 chars

Server-side via DataAnnotations + `IValidatableObject`.
Invalid input is not saved; form is re-shown with messages and entered values preserved.

## Basic Request Flow

```text
Browser -> ASP.NET Core -> Controller -> EF Core -> SQLite
  -> Controller -> Razor View -> HTML -> Browser
```

POST flow:

```text
Browser form -> POST -> Controller -> Model binding -> Validation
  -> EF Core -> SQLite -> Redirect -> GET details/dashboard
```

Dashboard flow: `Index()` queries SQLite, passes `List<Application>`
to `Index.cshtml`, which renders the HTML table.

## Adding a New Field (e.g. contact_person)

1. `Models/Application.cs` (property + validation)
2. `Data/ApplicationDbContext.cs` (column config if needed)
3. Recreate DB or add migration (delete `Data/placement_tracker.db` in dev, or `dotnet ef migrations add ...`)
4. `Views/Applications/Create.cshtml` + `Edit.cshtml` (form field)
5. `Views/Applications/Details.cshtml` (+ `Index.cshtml` if it should show in table)
6. `Controllers/ApplicationsController.cs` (copy field in Edit POST if you map manually)
7. Tests (validation + CRUD round-trip)

## Concept Checks (short answers)

- After entering the local URL, Kestrel routes `/` to `ApplicationsController.Index`,
  which queries SQLite and renders `Index.cshtml`.
- Create/delete use POST because GET must be safe/non-destructive
  (a plain link visit must never change data; browsers prefetch GETs).
- A redirect is an HTTP 302 telling the browser to GET another URL.
- Redirect after save (PRG) prevents duplicate submits on refresh.
- HTTP 404 means the numeric ID is valid but no row exists; we show `404.cshtml`.
- MVC picks controller/action from route attributes (`[HttpGet("/...")]`) + `{id}` params.
- Controller passes the entity/list as the Razor `@model`.
- Shared layout avoids duplicating nav/HTML on every page.
- CSS lives in `wwwroot` so it can be served as a static file.
- One `Application` instance = one row = one job application.
- Primary key (`Id`) uniquely identifies each row for view/edit/delete.
- `SaveChangesAsync` persists queued add/update/delete to SQLite.
- The SQLite file survives restart; InMemory would not.
