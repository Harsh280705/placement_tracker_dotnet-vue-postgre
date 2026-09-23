````markdown
# Placement Tracker (Vue 3 + ASP.NET Core Web API + PostgreSQL + NGINX)

A full-stack Placement Tracker migrated from ASP.NET Core MVC + Razor + SQLite to Vue 3 + ASP.NET Core Web API + PostgreSQL.

## Tech Stack

- Frontend: Vue 3 + Vite + Vue Router
- Backend: ASP.NET Core 8 Web API
- Database: PostgreSQL
- ORM: Entity Framework Core + Npgsql
- Reverse Proxy: NGINX
- Public Access: ngrok
- Testing: xUnit + Moq + EF InMemory

## Architecture

```text
Browser
   ↓
NGINX :8080
   ├── /       → Vue :5173
   └── /api/   → ASP.NET Core :5038
                         ↓
                    PostgreSQL :5432
````

## Project Structure

```text
backend/     → ASP.NET Core Web API
frontend/    → Vue 3 application
tests/       → API, integration & validation tests
nginx/       → NGINX configuration and scripts
```

## Requirements

* .NET 8 SDK
* Node.js + npm
* PostgreSQL 16
* NGINX
* ngrok (optional)

## Run Locally

### 1. Start Backend

```powershell
dotnet run --project backend/PlacementTracker.Api.csproj
```

API:

```text
http://localhost:5038
```

### 2. Start Frontend

```powershell
cd frontend
npm install
npm run dev
```

Vue:

```text
http://localhost:5173
```

### 3. Start NGINX

From the project root:

```powershell
.\nginx\start-nginx.ps1
```

Application:

```text
http://localhost:8080
```

NGINX routes `/` to Vue and `/api/` to the ASP.NET Core API.

## Run Tests

```powershell
dotnet test tests/PlacementTracker.Api.Tests.csproj
```

Tests use isolated InMemory databases and do not modify the real PostgreSQL database.

## API Endpoints

| Method | Endpoint                 | Purpose |
| ------ | ------------------------ | ------- |
| GET    | `/api/applications`      | Get all |
| GET    | `/api/applications/{id}` | Get one |
| POST   | `/api/applications`      | Create  |
| PUT    | `/api/applications/{id}` | Update  |
| DELETE | `/api/applications/{id}` | Delete  |

## ngrok

After the local application is working through NGINX:

```powershell
ngrok config add-authtoken <YOUR_TOKEN>
ngrok http 8080
```

Open the HTTPS URL provided by ngrok.

```text
Internet
   ↓
ngrok
   ↓
NGINX :8080
   ↓
Vue + ASP.NET Core API
   ↓
PostgreSQL
```

**Important:** Expose port `8080` through ngrok, not `5173` or `5038`.

## Ports

| Service          |             Port |
| ---------------- | ---------------: |
| PostgreSQL       |             5432 |
| ASP.NET Core API |             5038 |
| Vue              |             5173 |
| NGINX            |             8080 |
| ngrok            | Public HTTPS URL |

```
```
