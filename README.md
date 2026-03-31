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
└── workshop/
    ├── Basics/             # Module 2: 23 C# language topics
    ├── Fundamentals/       # Module 1: API concepts (docs)
    └── Challenges/         # Exercises: FixMe, ImplMe, Bank Transfer, Durable Temporal
```

---

## Modules

### Module 1 — API Fundamentals [`workshop/Fundamentals/`](workshop/Fundamentals/)

Language-agnostic API design concepts:

- [API Design](workshop/Fundamentals/ApiDesign/) — REST principles, naming, versioning
- [API Fundamentals](workshop/Fundamentals/ApiFundamentals/) — REST vs RPC, idempotency
- [Lifecycle & Deployment](workshop/Fundamentals/ApiLifecycleAndDeployment/) — versioning, containers, cloud
- [Security & Observability](workshop/Fundamentals/SecurityAndObservability/) — auth, logging, tracing
- [The Agentic Future](workshop/Fundamentals/TheAgenticFuture/) — APIs for AI agents, MCP

### Module 2 — C# Language Basics [`workshop/Basics/`](workshop/Basics/)

23 core C# / .NET concepts organized by theme:

- **Types & Memory**
    - [Value & Reference Types](workshop/Basics/ValueAndReferenceTypes/) — struct vs class, `ref`/`out`/`in`, `Span<T>`
    - [Parameters](workshop/Basics/Parameters/) — `params`, optional, named arguments
    - [Entities](workshop/Basics/Entity/) — `record`, `class`, `struct`, `record struct`
    - [Nullable Reference Types](workshop/Basics/NullableReferenceTypes/) — `?`, `?.`, `??`, `??=`, null guards
- **OOP & Patterns**
    - [Receivers](workshop/Basics/Receivers/) — Extension methods, C# 14 `extension` blocks
    - [Interfaces](workshop/Basics/Interface/) — Explicit interfaces, DI container
    - [Type Assertions & Casting](workshop/Basics/Casting/) — `is`, `as`, switch expression, pattern matching
    - [Composition & Inheritance](workshop/Basics/CompositionAndInheritance/) — Embedded resources, decorator pattern
    - [Generics](workshop/Basics/Generics/) — Generic methods, constraints, `default(T)`
- **Error Handling & Resources**
    - [Error Handling](workshop/Basics/ErrorHandling/) — Exceptions, `catch when`, `Result<T>`
    - [Disposable & Resource Management](workshop/Basics/Disposable/) — `IDisposable`, `using`, `IAsyncDisposable`
- **Code Organisation**
    - [Project Layout](workshop/Basics/Layout/) — `.sln`, `.csproj`, namespaces, project structure
    - [Initialization](workshop/Basics/Initialization/) — Static constructors, `[ModuleInitializer]`, `Lazy<T>`
- **Data & Queries**
    - [LINQ](workshop/Basics/Linq/) — `Where`, `Select`, `GroupBy`, deferred execution
- **Testing**
    - [Testing](workshop/Basics/Testing/) — xUnit: `[Fact]`, `[Theory]`, `[InlineData]`
    - [FluentAssertions](workshop/Basics/FluentAssertions/) — `.Should().Be()`, `.Should().Throw<T>()`
    - [Mocking](workshop/Basics/Mocking/) — Moq — `Mock<T>`, `Setup`, `Verify`
- **HTTP Services**
    - [HTTP Client & Server](workshop/Basics/Http/) — Minimal APIs, `HttpClient`, `IHttpClientFactory`
    - [HTTP Testing](workshop/Basics/HttpTest/) — `WebApplicationFactory<T>`
    - [Benchmark](workshop/Basics/Benchmark/) — BenchmarkDotNet
- **Concurrency & Context**
    - [Concurrency](workshop/Basics/Concurrency/) — `async/await`, `Task`, `Channel<T>`
    - [Context](workshop/Basics/Context/) — `CancellationToken`, timeouts, `AsyncLocal<T>`
- **Compilation & Platform**
    - [Conditional Compilation](workshop/Basics/ConditionalCompilation/) — `#if DEBUG`, `DefineConstants`, `RuntimeInformation`

Navigate to the [Module 2 Overview](workshop/Basics/README.md) to find code examples and documentation.

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

- [Durable Transfer Quest](workshop/Challenges/Temporal/README.md) — Build a high-value transfer workflow with human-in-the-loop approval.
- [Official Temporal Developer Skill](https://github.com/temporalio/skill-temporal-developer) — Use this expert skill to guide your implementation.

---

## Challenges [`workshop/Challenges/`](workshop/Challenges/)

- [Challenges Overview](workshop/Challenges/README.md)
- [Basics Challenges](workshop/Challenges/Basics/README.md) — Detective mysteries covering core C# concepts.
- [.NET Bank Transfer Quest](workshop/Challenges/Bank/) — Build the `POST /v1/transfers` API endpoint end-to-end.
- [Durable Transfer Quest](workshop/Challenges/Temporal/README.md) — Build a robust distributed transaction with Temporal.

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

> Note: Solution branches are available for each of the challenges
