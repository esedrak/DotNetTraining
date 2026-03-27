# DotNetTraining — .NET 10 Workshop

A hands-on workshop for learning **C# 14 / .NET 10 / ASP.NET Core 10**

---

## Prerequisites

| Tool | Version | Install |
|------|---------|---------|
| .NET SDK | 10.0+ | [dotnet.microsoft.com](https://dotnet.microsoft.com/download) |
| Docker Desktop or colima | Latest | [docker.com](https://www.docker.com/products/docker-desktop/) / [colima.com](https://github.com/abiosoft/colima) |
| IDE | Any | VS Code + C# DevKit, JetBrains Rider, or Visual Studio |

```bash
# Verify
dotnet --version   # should print 10.x.x
docker --version
```

---

## Getting Started

```bash
git clone <repo-url>
cd DotNetTraining

# Restore NuGet packages
dotnet restore

# Build all projects
dotnet build

# Run all tests
dotnet test

# Start infrastructure (Postgres, Temporal, WireMock, Jaeger)
make infra-up

# Create EF Core migrations (first time only - if not already created)
dotnet ef migrations add InitialCreate --project src/Bank.Repository

# Apply migrations to create database tables
make db-migrate

# Run the Bank API
make run-bank-api
```

---

## Workshop Structure

```
DotNetTraining/
├── src/                    # Production code (4 service groups)
│   ├── Hello/              # Console app — warm-up
│   ├── Bank.*/             # Module 3: Layered bank API
│   ├── Temporal.*/         # Module 4: Workflow orchestration
│   └── Shared/             # Shared libraries
├── tests/                  # xUnit test projects
├── internal/
│   ├── Basics/             # Module 2: 20 C# language topics
│   └── Fundamentals/       # Module 1: API concepts (docs)
└── internal/Challenges/    # Exercises: FixMe, ImplMe, Bank Transfer
```

---

## Modules

### Module 1 — API Fundamentals [`internal/Fundamentals/`](internal/Fundamentals/)

Language-agnostic API design concepts:

- [API Design](internal/Fundamentals/ApiDesign/) — REST principles, naming, versioning
- [API Fundamentals](internal/Fundamentals/ApiFundamentals/) — REST vs RPC, idempotency
- [Lifecycle & Deployment](internal/Fundamentals/ApiLifecycleAndDeployment/) — versioning, containers, cloud
- [Security & Observability](internal/Fundamentals/SecurityAndObservability/) — auth, logging, tracing
- [The Agentic Future](internal/Fundamentals/TheAgenticFuture/) — APIs for AI agents, MCP

### Module 2 — C# Language Basics [`internal/Basics/`](internal/Basics/)

20 topics, each with README + code + tests:

| Topic | Key Concepts |
|-------|-------------|
| [Pointers](internal/Basics/Pointers/) | `ref`/`out`/`in`, value vs reference types |
| [Casting](internal/Basics/Casting/) | `is`, `as`, switch expression, pattern matching |
| [Parameters](internal/Basics/Parameters/) | `params`, optional, named arguments |
| [Entity](internal/Basics/Entity/) | `record`, `class`, `struct`, `record struct` |
| [Layout](internal/Basics/Layout/) | `.sln`, `.csproj`, namespaces, project structure |
| [Embedding](internal/Basics/Embedding/) | Embedded resources, composition |
| [Receivers](internal/Basics/Receivers/) | Extension methods, C# 14 `extension` blocks |
| [Init](internal/Basics/Init/) | Static constructors, `[ModuleInitializer]`, `Lazy<T>` |
| [ErrorHandling](internal/Basics/ErrorHandling/) | Exceptions, `catch when`, `Result<T>` |
| [Interface](internal/Basics/Interface/) | Explicit interfaces, DI container |
| [Concurrency](internal/Basics/Concurrency/) | `async/await`, `Task`, `Channel<T>` |
| [Context](internal/Basics/Context/) | `CancellationToken`, timeouts, `AsyncLocal<T>` |
| [Testing](internal/Basics/Testing/) | xUnit: `[Fact]`, `[Theory]`, `[InlineData]` |
| [Testify](internal/Basics/Testify/) | FluentAssertions |
| [Benchmark](internal/Basics/Benchmark/) | BenchmarkDotNet |
| [Http](internal/Basics/Http/) | Minimal APIs, `HttpClient`, `IHttpClientFactory` |
| [HttpTest](internal/Basics/HttpTest/) | `WebApplicationFactory<T>` |
| [Generics](internal/Basics/Generics/) | Generic methods, constraints, `default(T)` |
| [Mocking](internal/Basics/Mocking/) | Moq — `Mock<T>`, `Setup`, `Verify` |
| [BuildTags](internal/Basics/BuildTags/) | `#if DEBUG`, `DefineConstants`, `RuntimeInformation` |

### Module 3 — Bank Service [`src/Bank.*`](src/)

A production-grade layered banking API:

```
Bank.Domain     → entities, exceptions (no dependencies)
Bank.Repository → EF Core + Postgres (depends on Domain)
Bank.Service    → business logic (depends on Repository + Domain)
Bank.Api        → ASP.NET Core controllers (depends on Service)
Bank.Cli        → System.CommandLine client
```

**Endpoints:**
- `GET /v1/accounts` — list accounts
- `GET /v1/accounts/{id}` — get account
- `POST /v1/accounts` — create account
- `GET /v1/transfers` — list transfers
- `GET /v1/transfers/{id}` — get transfer
- `POST /v1/transfers` — create transfer

### Module 4 — Temporal Orchestration [`src/Temporal.*`](src/)

Durable workflow orchestration using the [Temporal .NET SDK](https://github.com/temporalio/sdk-dotnet):

- **OrderWorkflow** — full order lifecycle with child workflows and signals
- **PaymentWorkflow** — payment processing child workflow
- **Activities** — validate, process payment, pick, ship

---

## Challenges [`internal/Challenges/`](internal/Challenges/)

| Challenge | Description |
|-----------|-------------|
| [FixMe](internal/Challenges/Basics/FixMe/) | Fix intentionally buggy C# code |
| [ImplMe](internal/Challenges/Basics/ImplMe/) | Implement method stubs |
| [Bank Transfer Quest](internal/Challenges/Bank/) | Implement `POST /v1/transfers` end-to-end |

---

## Go → .NET Concept Map

| Go | .NET / C# |
|----|-----------|
| `go.mod` | `.sln` + `.csproj` |
| goroutines | `async/await`, `Task.Run()` |
| `chan T` | `Channel<T>` |
| `context.Context` | `CancellationToken` |
| `error` interface | `Exception` hierarchy |
| Implicit interfaces | Explicit `: IInterface` |
| `testing.T` | xUnit `[Fact]` |
| `testify/assert` | FluentAssertions |
| `httptest` | `WebApplicationFactory<T>` |
| Mockery | Moq |
| Gin | ASP.NET Core |
| go-jet | Entity Framework Core |
| Cobra | System.CommandLine |
| Viper | `IConfiguration` / `IOptions<T>` |
| slog | `ILogger<T>` / Serilog |
| `//go:build` | `#if`, `<DefineConstants>` |
| `go:embed` | `<EmbeddedResource>` |
| `testing.B` | BenchmarkDotNet |

---

## Useful Commands

```bash
make build          # Build all projects
make test           # Run all tests
make run-bank-api   # Start the bank API
make run-hello      # Run Hello world
make infra-up       # Start Docker services
make infra-down     # Stop Docker services
make db-migrate     # Run EF Core migrations
make clean          # Clean build artifacts
make help           # Show all targets
```
