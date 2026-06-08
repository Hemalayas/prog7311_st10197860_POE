# TechMove.GLMS

A .NET 8 ASP.NET Core MVC web application for managing clients, contracts, and service requests.

---

## Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- SQL Server (LocalDB is sufficient — included with Visual Studio)

---

## Getting Started

### 1. Clone the repository

```bash
git clone <repo-url>
cd TechMove.GLMS
```

### 2. Configure the database connection

The default connection string in `TechMove.GLMS/appsettings.json` points to LocalDB:

```json
"ConnectionStrings": {
  "DefaultConnection": "Server=(localdb)\\mssqllocaldb;Database=TechMoveGLMS;Trusted_Connection=True;"
}
```

Update this if you are using a different SQL Server instance.

### 3. Apply database migrations

```bash
cd TechMove.GLMS
dotnet ef database update
```

This creates the `TechMoveGLMS` database and seeds initial test data (2 clients, 3 contracts, 3 service requests).

### 4. Run the application

```bash
dotnet run --project TechMove.GLMS
```

The app will be available at `https://localhost:5001` (or the port shown in the console).

---

## Running Tests

```bash
dotnet test
```

All 13 unit tests should pass. The test project covers:
- Currency conversion (`CurrencyCalculationService`)
- File validation (`FileValidationService`)
- Workflow validation (`WorkflowValidationService`)

---

## Building the Solution

```bash
dotnet build
```

---

## Project Structure

```
TechMove.GLMS/
├── TechMove.GLMS.sln
├── TechMove.GLMS/               # Main MVC app
│   ├── Controllers/             # HTTP controllers (no direct DbContext access)
│   ├── Data/                    # EF Core DbContext
│   ├── Interfaces/              # Repository + observer interfaces
│   ├── Models/                  # Entity classes (Client, Contract, ServiceRequest)
│   ├── Repositories/            # EF Core repository implementations
│   ├── Services/                # Business services + observer implementations
│   ├── ViewModels/              # View-specific models
│   ├── Views/                   # Razor views (Bootstrap 5)
│   ├── wwwroot/                 # Static assets
│   ├── Program.cs               # App startup, DI registration
│   └── appsettings.json
└── TechMove.GLMS.Tests/         # xUnit test project
```

---

## Key Features

- **Clients** — create, view, edit, and delete clients
- **Contracts** — full CRUD with PDF upload, status tracking, and date-range search
- **Service Requests** — create service requests against active/draft contracts with live USD → ZAR conversion
- **Contract status badges** — colour-coded: Active (green), Expired (red), Draft (yellow), OnHold (grey)
- **Observer pattern** — email notification, audit log, and dashboard observers fire on contract status changes
- **Factory pattern** — `ServiceRequestFactory` maps service type to standardised service level descriptions
- **Repository pattern** — all data access goes through repository interfaces; controllers never touch `DbContext` directly

---

## Design Patterns Used

| Pattern | Where |
|---|---|
| Repository | `IClientRepository`, `IContractRepository`, `IServiceRequestRepository` |
| Observer | `IContractObserver` + `EmailNotificationObserver`, `AuditLogObserver`, `DashboardObserver` |
| Factory | `ServiceRequestFactory` |

---

## EF Core Migrations

To add a new migration after model changes:

```bash
cd TechMove.GLMS
dotnet ef migrations add <MigrationName>
dotnet ef database update
```
