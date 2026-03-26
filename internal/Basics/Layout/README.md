# 📁 .NET Project Organization

.NET uses **solutions** (`.sln`) and **projects** (`.csproj`) to organize code. This replaces Go's `go.mod`, `cmd/`, `internal/`, and `pkg/` conventions.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **Solution (`.sln`)** | Groups related projects — equivalent to a Go module workspace |
| **Project (`.csproj`)** | A single compilable unit — equivalent to a Go package |
| **`internal` access modifier** | Visible only within the same assembly — enforces module boundaries |
| **Namespace** | Logical grouping within a project (≈ Go package, by convention matches folder) |
| **`Directory.Build.props`** | Shared MSBuild properties across all projects (centralized config) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `go.mod` | `.sln` + `.csproj` |
| `go.sum` | NuGet `packages.lock.json` |
| `cmd/` | Executable projects (`Console`, `WebAPI`, `Worker`) |
| `internal/` | `internal` access modifier + project boundaries |
| `pkg/` | Shared class libraries referenced by multiple projects |
| Package = directory | Namespace ≈ directory (by convention) |
| `go build ./...` | `dotnet build` |
| `go test ./...` | `dotnet test` |
| `go run cmd/app/main.go` | `dotnet run --project src/App` |

---

## 3. This Repo's Layout

```
DotNetTraining/
├── DotNetTraining.sln          # Solution — groups all projects
├── Directory.Build.props       # Shared settings (TargetFramework, Nullable, etc.)
│
├── src/                        # Production code
│   ├── Hello/                  # Console app (cmd/hello equivalent)
│   ├── Bank.Domain/            # Class library — pure domain types (no dependencies)
│   ├── Bank.Repository/        # Class library — data access (depends on Domain)
│   ├── Bank.Service/           # Class library — business logic
│   ├── Bank.Api/               # ASP.NET Core Web API (depends on Service + Domain)
│   ├── Bank.Cli/               # Console app — CLI client
│   ├── Temporal.*/             # Temporal workflow projects
│   └── Shared/                 # Shared utilities (pkg/ equivalent)
│
├── tests/                      # Test projects
│   ├── Bank.Tests/             # xUnit tests for Bank.*
│   └── Basics.Tests/           # xUnit tests for Basics examples
│
└── internal/                   # Workshop learning materials (not .NET internal keyword)
    ├── Basics/                 # 20 topic modules
    └── Fundamentals/           # Docs-heavy API concepts
```

---

## 4. Layered Architecture (Dependency Rules)

```mermaid
flowchart TD
    API["Bank.Api\n(ASP.NET Core)"]
    CLI["Bank.Cli\n(Console)"]
    SVC["Bank.Service\n(Business Logic)"]
    REPO["Bank.Repository\n(EF Core)"]
    DOM["Bank.Domain\n(Entities, Exceptions)"]

    API --> SVC
    API --> DOM
    CLI --> DOM
    SVC --> REPO
    SVC --> DOM
    REPO --> DOM
```

**The rule**: dependencies flow inward. Domain has no dependencies. Repository depends on Domain. Service depends on both. API depends on Service.

---

## 📚 Further Reading

- [.NET project SDK](https://learn.microsoft.com/en-us/dotnet/core/project-sdk/overview)
- [Solution files](https://learn.microsoft.com/en-us/visualstudio/ide/solutions-and-projects-in-visual-studio)
- [Clean Architecture in .NET](https://learn.microsoft.com/en-us/dotnet/architecture/modern-web-apps-azure/common-web-application-architectures)
