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
└── internal/Challenges/    # Exercises: FixMe, ImplMe, Bank Transfer, Durable Temporal
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

Dive into the building blocks of C# by exploring the following topics:

- [The Basics & The Mental Shift](internal/Basics/README.md#the-basics--the-mental-shift)
    - [Pointers](internal/Basics/Pointers/) — `ref`/`out`/`in`, value vs reference types
    - [Parameters](internal/Basics/Parameters/) — `params`, optional, named arguments
    - [Error Handling](internal/Basics/ErrorHandling/) — Exceptions, `catch when`, `Result<T>`
- [Defining Data & Behaviour](internal/Basics/README.md#defining-data--behaviour)
    - [Entities](internal/Basics/Entity/) — `record`, `class`, `struct`, `record struct`
    - [Receivers](internal/Basics/Receivers/) — Extension methods, C# 14 `extension` blocks
    - [Interfaces](internal/Basics/Interface/) — Explicit interfaces, DI container
    - [Type Assertions & Casting](internal/Basics/Casting/) — `is`, `as`, switch expression, pattern matching
    - [Embedding](internal/Basics/Embedding/) — Embedded resources, composition
- [Code Organisation](internal/Basics/README.md#code-organisation)
    - [Package Layout](internal/Basics/Layout/) — `.sln`, `.csproj`, namespaces, project structure
    - [Init](internal/Basics/Init/) — Static constructors, `[ModuleInitializer]`, `Lazy<T>`
- [Testing](internal/Basics/README.md#testing)
    - [Testing](internal/Basics/Testing/) — xUnit: `[Fact]`, `[Theory]`, `[InlineData]`
    - [Testify](internal/Basics/Testify/) — FluentAssertions
    - [Mocking](internal/Basics/Mocking/) — Moq — `Mock<T>`, `Setup`, `Verify`
- [HTTP Services](internal/Basics/README.md#http-services)
    - [HTTP Client & Server](internal/Basics/Http/) — Minimal APIs, `HttpClient`, `IHttpClientFactory`
    - [HTTP Testing](internal/Basics/HttpTest/) — `WebApplicationFactory<T>`
    - [Benchmark](internal/Basics/Benchmark/) — BenchmarkDotNet
- [Concurrency & Context](internal/Basics/README.md#concurrency--context)
    - [Concurrency](internal/Basics/Concurrency/) — `async/await`, `Task`, `Channel<T>`
    - [Context](internal/Basics/Context/) — `CancellationToken`, timeouts, `AsyncLocal<T>`
- [Advanced Features](internal/Basics/README.md#advanced-features)
    - [Generics](internal/Basics/Generics/) — Generic methods, constraints, `default(T)`
    - [Build Tags](internal/Basics/BuildTags/) — `#if DEBUG`, `DefineConstants`, `RuntimeInformation`

Navigate to the [Module 2 Overview](internal/Basics/README.md) to find code examples and documentation.

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

Discover reliable, durable execution patterns for long-running workflows:

- [Temporal Module Overview](src/Temporal.Worker/) — Worker implementation and workflow registration.
- **OrderWorkflow** — full order lifecycle with child workflows and signals
- **PaymentWorkflow** — payment processing child workflow
- **Activities** — validate, process payment, pick, ship

### Module 5: Agentic .NET & Durable Workflows

Live demonstration and hands-on challenge: leveraging AI agents for ultra-efficient .NET development.

- [Durable Transfer Quest](internal/Challenges/Temporal/README.md) — Build a high-value transfer workflow with human-in-the-loop approval.
- [Official Temporal Developer Skill](https://github.com/temporalio/skill-temporal-developer) — Use this expert skill to guide your implementation.

---

## Challenges [`internal/Challenges/`](internal/Challenges/)

- [Challenges Overview](internal/Challenges/README.md)
- [Day 1: Basics Challenges](internal/Challenges/Basics/README.md) — Detective mysteries covering core C# concepts.
- [Day 2: .NET Bank Transfer Quest](internal/Challenges/Bank/) — Build the `POST /v1/transfers` API endpoint end-to-end.
- [Day 2: Durable Transfer Quest](internal/Challenges/Temporal/README.md) — Build a robust distributed transaction with Temporal.

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
