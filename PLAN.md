# CSharpTraining — Conversion Plan

> Convert the [GoTraining](../GoTraining) workshop into an equivalent **C# / .NET 10 / ASP.NET Core 10** training course.
> The GoTraining repo remains untouched. This is a brand-new standalone project.

**Target Stack:** .NET 10 (LTS) | C# 14 | ASP.NET Core 10 | EF Core 10 | xUnit | Temporal .NET SDK

---

## Table of Contents

1. [Project Structure](#phase-0--project-structure)
2. [Infrastructure & Tooling](#phase-1--infrastructure--tooling)
3. [Module 1 — API Fundamentals (Docs)](#phase-2--module-1--api-fundamentals-docs)
4. [Module 2 — C# Language Basics](#phase-3--module-2--c-language-basics)
5. [Module 3 — Bank Service (ASP.NET Core)](#phase-4--module-3--bank-service-aspnet-core)
6. [Module 4 — Temporal Orchestration](#phase-5--module-4--temporal-orchestration)
7. [Challenges](#phase-6--challenges)
8. [Documentation](#phase-7--documentation)
9. [CI/CD](#phase-8--cicd)
10. [Concept Mapping Reference](#concept-mapping-reference)
11. [NuGet Packages](#nuget-packages)

---

## Phase 0 — Project Structure

Create the .NET solution with the following layout. This maps Go conventions (`cmd/`, `internal/`, `pkg/`) to idiomatic .NET conventions (`src/`, `tests/`, shared class libraries).

```
CSharpTraining/
├── CSharpTraining.sln                        # Solution file (replaces go.mod)
├── PLAN.md                                   # This file
├── README.md                                 # Workshop overview
├── docker-compose.yaml                       # Postgres, Temporal, WireMock, Jaeger
├── Makefile                                  # Wraps dotnet CLI commands
├── .editorconfig                             # Code style (replaces .golangci.yaml)
├── Directory.Build.props                     # Shared project settings
├── .gitignore
│
├── src/
│   │
│   │  # ── Hello World ──────────────────────
│   ├── Hello/                                # Console app  (was: cmd/hello)
│   │   ├── Hello.csproj
│   │   ├── Program.cs
│   │   └── Dockerfile
│   │
│   │  # ── Bank Service (Module 3) ──────────
│   ├── Bank.Domain/                          # Class library (was: internal/bank/domain)
│   │   ├── Bank.Domain.csproj
│   │   ├── Account.cs
│   │   ├── Transaction.cs
│   │   ├── Transfer.cs
│   │   └── Exceptions/
│   │       ├── AccountNotFoundException.cs
│   │       └── InsufficientFundsException.cs
│   │
│   ├── Bank.Repository/                      # Class library (was: internal/bank/repository)
│   │   ├── Bank.Repository.csproj
│   │   ├── IBankRepository.cs
│   │   ├── BankDbContext.cs                  # EF Core DbContext
│   │   └── PostgresBankRepository.cs
│   │
│   ├── Bank.Service/                         # Class library (was: internal/bank/service)
│   │   ├── Bank.Service.csproj
│   │   ├── IBankService.cs
│   │   └── BankService.cs
│   │
│   ├── Bank.Api/                             # ASP.NET Core Web API (was: cmd/bank/server + api/)
│   │   ├── Bank.Api.csproj
│   │   ├── Program.cs
│   │   ├── appsettings.json
│   │   ├── Dockerfile
│   │   ├── Controllers/
│   │   │   ├── AccountController.cs
│   │   │   └── TransferController.cs
│   │   └── Middleware/
│   │       ├── AuthMiddleware.cs
│   │       ├── LoggingMiddleware.cs
│   │       └── TracingMiddleware.cs
│   │
│   ├── Bank.Cli/                             # Console app (was: cmd/bank/cli)
│   │   ├── Bank.Cli.csproj
│   │   └── Program.cs
│   │
│   │  # ── Temporal (Module 4) ──────────────
│   ├── Temporal.Domain/                      # Class library (was: internal/temporal/order)
│   │   ├── Temporal.Domain.csproj
│   │   ├── Order.cs
│   │   └── OrderStatus.cs
│   │
│   ├── Temporal.Workflows/                   # Class library (was: internal/temporal/workflows + activities)
│   │   ├── Temporal.Workflows.csproj
│   │   ├── Workflows/
│   │   │   ├── OrderWorkflow.cs
│   │   │   └── PaymentWorkflow.cs
│   │   ├── Activities/
│   │   │   ├── OrderActivities.cs
│   │   │   └── InventoryActivities.cs
│   │   └── Encryption/
│   │       ├── PayloadCodec.cs
│   │       └── EncryptionHelper.cs
│   │
│   ├── Temporal.Worker/                      # Worker Service (was: cmd/temporal/worker)
│   │   ├── Temporal.Worker.csproj
│   │   └── Program.cs
│   │
│   ├── Temporal.Client/                      # Console app (was: cmd/temporal/client)
│   │   ├── Temporal.Client.csproj
│   │   └── Program.cs
│   │
│   │  # ── Shared Libraries ─────────────────
│   └── Shared/
│       ├── Shared.Api/                       # (was: pkg/api/apierror)
│       │   ├── Shared.Api.csproj
│       │   └── ApiError.cs
│       └── Shared.Http/                      # (was: pkg/http)
│           ├── Shared.Http.csproj
│           └── HttpClientExtensions.cs
│
├── internal/
│   │
│   │  # ── Basics (Module 2) ────────────────
│   ├── Basics/                               # One folder per topic, each with README + code + tests
│   │   ├── Casting/
│   │   ├── Concurrency/
│   │   ├── Context/
│   │   ├── Embedding/
│   │   ├── Entity/
│   │   ├── ErrorHandling/
│   │   ├── Generics/
│   │   ├── Http/
│   │   ├── HttpTest/
│   │   ├── Init/
│   │   ├── Interface/
│   │   ├── Layout/
│   │   ├── Mocking/
│   │   ├── Parameters/
│   │   ├── Pointers/
│   │   ├── Receivers/
│   │   ├── Testing/
│   │   ├── Testify/
│   │   ├── Benchmark/
│   │   ├── BuildTags/
│   │   └── README.md
│   │
│   │  # ── Fundamentals (Module 1) ──────────
│   ├── Fundamentals/                         # Docs-heavy, mostly language-agnostic
│   │   ├── ApiDesign/
│   │   ├── ApiFundamentals/
│   │   ├── ApiLifecycleAndDeployment/
│   │   ├── SecurityAndObservability/
│   │   ├── TheAgenticFuture/
│   │   └── README.md
│   │
│   │  # ── Challenges ───────────────────────
│   └── Challenges/
│       ├── Basics/
│       │   ├── FixMe/
│       │   └── ImplMe/
│       ├── Bank/
│       └── README.md
│
├── tests/
│   ├── Hello.Tests/
│   ├── Bank.Tests/
│   ├── Temporal.Tests/
│   └── Basics.Tests/                         # Tests for the Basics modules
│
├── docs/
│   └── openapi/solution/
│
├── migration/                                # Same SQL migrations (reusable)
├── wiremock/                                 # Same WireMock configs (reusable)
├── config/
│   └── temporal/
│       ├── client/local/appsettings.json
│       └── worker/local/appsettings.json
│
└── .github/
    └── workflows/
        └── build.yml
```

### Tasks

- [ ] `dotnet new sln` — create solution
- [ ] Create each project with `dotnet new` (classlib, web, console, worker)
- [ ] Add all projects to the solution
- [ ] Add `Directory.Build.props` with shared settings (TargetFramework `net10.0`, Nullable enable, ImplicitUsings enable)
- [ ] Add `.editorconfig` for code style
- [ ] Add `.gitignore` (dotnet template)
- [ ] `git init`

---

## Phase 1 — Infrastructure & Tooling

Reuse the GoTraining infrastructure (all services are language-agnostic) and create .NET-specific build tooling.

### docker-compose.yaml (copy & adapt)

| Service    | Image                        | Ports              | Purpose                |
|------------|------------------------------|--------------------|------------------------|
| postgres   | `postgres:15-alpine`         | 5432               | Database (rename db to `csharpbank`) |
| temporal   | `temporalio/temporal:1.5.1`  | 7233 (gRPC), 8233 (UI) | Workflow engine     |
| wiremock   | `wiremock/wiremock:3.3.1`    | 8081               | Mock APIs              |
| jaeger     | `jaegertracing/all-in-one:1.60` | 16686 (UI), 4318 (OTLP) | Distributed tracing |

### Makefile targets

```makefile
build:          dotnet build
test:           dotnet test
run-bank-api:   dotnet run --project src/Bank.Api
run-bank-cli:   dotnet run --project src/Bank.Cli
run-hello:      dotnet run --project src/Hello
run-worker:     dotnet run --project src/Temporal.Worker
run-client:     dotnet run --project src/Temporal.Client
db-up:          docker compose up -d postgres
infra-up:       docker compose up -d
infra-down:     docker compose down
db-migrate:     dotnet ef database update --project src/Bank.Repository
clean:          dotnet clean && rm -rf bin/ obj/
```

### Dockerfile (multi-stage, equivalent to Go's scratch image)

```dockerfile
# Build
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src
COPY . .
RUN dotnet publish src/Bank.Api -c Release -o /app

# Runtime
FROM mcr.microsoft.com/dotnet/aspnet:10.0
WORKDIR /app
COPY --from=build /app .
ENTRYPOINT ["dotnet", "Bank.Api.dll"]
```

### Tasks

- [ ] Create `docker-compose.yaml` (adapt from GoTraining)
- [ ] Create `Makefile` with dotnet CLI wrappers
- [ ] Create `Dockerfile` for Hello and Bank.Api
- [ ] Copy `wiremock/` directory from GoTraining (reusable as-is)
- [ ] Copy `migration/` SQL files from GoTraining (adapt if needed for EF Core)

---

## Phase 2 — Module 1: API Fundamentals (Docs)

These are primarily **documentation/README** modules that are mostly language-agnostic. Minimal code changes needed.

### Folders to create

| Folder                               | GoTraining Source                               | Changes Needed |
|--------------------------------------|------------------------------------------------|----------------|
| `internal/Fundamentals/ApiDesign/`   | `internal/fundamentals/api-design/`            | Minor: update any Go snippets to C# |
| `internal/Fundamentals/ApiFundamentals/` | `internal/fundamentals/api-fundamentals/`  | Minor: update tool references |
| `internal/Fundamentals/ApiLifecycleAndDeployment/` | `internal/fundamentals/api-lifecycle-and-deployment/` | Update Docker examples to .NET |
| `internal/Fundamentals/SecurityAndObservability/` | `internal/fundamentals/security-and-observability/` | Replace slog → ILogger, update OTel refs |
| `internal/Fundamentals/TheAgenticFuture/` | `internal/fundamentals/the-agentic-future/` | Minimal changes |

### Tasks

- [ ] Copy each README and adapt Go-specific references
- [ ] Update code snippets from Go to C# where present
- [ ] Replace tool references: `slog` → `ILogger<T>`/Serilog, `otel` → OpenTelemetry.NET
- [ ] Update container examples to .NET Dockerfiles

---

## Phase 3 — Module 2: C# Language Basics

The **largest phase**. Each Go basics topic maps to a C# equivalent. Every folder gets:
1. `README.md` — Conceptual explanation with Mermaid diagrams
2. One or more `.cs` files — Implementation examples
3. Corresponding tests in `tests/Basics.Tests/`

### Topic-by-Topic Mapping

#### 3.1 Pointers → Value Types vs Reference Types

| Go | C# |
|----|-----|
| `*int`, `&x` | `ref`, `out`, `in` keywords |
| Pointer dereferencing | No raw pointers in safe C# |
| `nil` pointer | `null` reference, `Nullable<T>` |
| Stack vs heap | Value types (struct) vs reference types (class) |
| N/A | `Span<T>`, `Memory<T>` for safe memory access |

**Files:** `ValueVsReference.cs`, `RefOutIn.cs`, `SpanExamples.cs`, `README.md`

#### 3.2 Casting → Pattern Matching

| Go | C# |
|----|-----|
| Type assertion `x.(Type)` | `is`, `as`, explicit cast |
| Type switch | `switch` expression with pattern matching |
| `interface{}` / `any` | `object`, generics |

**Files:** `PatternMatching.cs`, `TypeChecking.cs`, `README.md`

#### 3.3 Parameters

| Go | C# |
|----|-----|
| Pass by value (default) | Pass by value (default) |
| Pass by pointer `*T` | `ref`, `out`, `in` |
| Variadic `...T` | `params T[]` / `params ReadOnlySpan<T>` |
| N/A | Optional parameters with defaults |
| N/A | Named arguments |

**Files:** `ParameterPassing.cs`, `RefOutExamples.cs`, `ParamsExamples.cs`, `README.md`

#### 3.4 Entity → Records, Classes, Structs

| Go | C# |
|----|-----|
| `type X struct {}` | `class`, `record`, `struct`, `record struct` |
| Factory functions `NewX()` | Constructors, primary constructors (C# 12+) |
| Struct tags (json) | `[JsonPropertyName]`, `System.Text.Json` attributes |
| Value equality (manual) | `record` gives value equality for free |
| N/A | `init` properties, `required` keyword |

**Files:** `EntityExamples.cs`, `RecordExamples.cs`, `README.md`

#### 3.5 Layout → .NET Project Organization

| Go | C# |
|----|-----|
| `cmd/` | Executable projects (Console, Web API, Worker) |
| `internal/` | `internal` access modifier, project boundaries |
| `pkg/` | Shared class libraries |
| `go.mod` | `.csproj` + `.sln` |
| Package = directory | Namespace ≈ directory (by convention) |

**Files:** `README.md` (mostly documentation)

#### 3.6 Embedding → Embedded Resources

| Go | C# |
|----|-----|
| `//go:embed` directive | `<EmbeddedResource>` in .csproj |
| `embed.FS` | `Assembly.GetManifestResourceStream()` |
| Struct embedding (composition) | Inheritance + composition |

**Files:** `EmbeddedResources.cs`, `Composition.cs`, `README.md`

#### 3.7 Receivers → Methods & Extension Methods

| Go | C# |
|----|-----|
| Value receiver `func (v T)` | Instance method (on struct = copy) |
| Pointer receiver `func (p *T)` | Instance method (on class = by ref) |
| N/A | Extension methods (`this` parameter) |
| N/A | C# 14 `extension` blocks (new!) |
| N/A | Static methods |

**Files:** `MethodReceivers.cs`, `ExtensionMethods.cs`, `ExtensionBlocks.cs` (C# 14), `README.md`

#### 3.8 Init → Static Constructors & Module Initializers

| Go | C# |
|----|-----|
| `func init()` | Static constructor `static ClassName()` |
| Package-level init | `[ModuleInitializer]` attribute |
| Execution order | Static field init → static ctor → instance |

**Files:** `StaticConstructors.cs`, `ModuleInitializer.cs`, `README.md`

#### 3.9 Error Handling → Exceptions & Result Pattern

| Go | C# |
|----|-----|
| `error` interface | `Exception` base class |
| `errors.New()` | `new CustomException()` |
| `fmt.Errorf("%w", err)` | `throw new X("msg", innerException)` |
| `errors.Is()` | `catch (SpecificException)`, `when` filter |
| `errors.As()` | `catch (T ex)` with type pattern |
| Sentinel errors | Custom exception types |
| Multi-return `(T, error)` | `Result<T>` pattern (alternative to exceptions) |

**Files:** `ExceptionHandling.cs`, `CustomExceptions.cs`, `ResultPattern.cs`, `README.md`

#### 3.10 Interfaces

| Go | C# |
|----|-----|
| Implicit implementation | Explicit `interface` keyword + `: IInterface` |
| Consumer-side definition | Defined in consuming assembly (same pattern) |
| Interface composition | Interface inheritance |
| Empty interface `any` | `object` or generics |
| N/A | Default interface methods (C# 8+) |
| N/A | DI container registration |

**Files:** `InterfaceBasics.cs`, `DependencyInjection.cs`, `README.md`

#### 3.11 Concurrency → async/await, Tasks, Channels

| Go | C# |
|----|-----|
| `go func()` (goroutine) | `Task.Run()`, `async/await` |
| `chan T` (channel) | `Channel<T>` (`System.Threading.Channels`) |
| `select` | `Task.WhenAny()`, channel multiplexing |
| `sync.WaitGroup` | `Task.WhenAll()` |
| `sync.Mutex` | `lock`, `SemaphoreSlim` |
| `sync.RWMutex` | `ReaderWriterLockSlim` |
| `sync.Once` | `Lazy<T>` |
| `atomic` | `Interlocked` |

**Files:** `AsyncAwait.cs`, `Channels.cs`, `TaskParallelism.cs`, `Synchronization.cs`, `README.md`

#### 3.12 Context → CancellationToken

| Go | C# |
|----|-----|
| `context.Background()` | `CancellationToken.None` |
| `context.WithCancel()` | `new CancellationTokenSource()` |
| `context.WithTimeout()` | `new CancellationTokenSource(TimeSpan)` |
| `context.WithValue()` | `AsyncLocal<T>`, `IHttpContextAccessor` |
| Pass as first arg | Pass as last arg (by convention) |

**Files:** `CancellationExamples.cs`, `TimeoutExamples.cs`, `README.md`

#### 3.13 Testing → xUnit

| Go | C# |
|----|-----|
| `func TestXxx(t *testing.T)` | `[Fact]` methods |
| Table-driven tests | `[Theory]` + `[InlineData]` / `[MemberData]` |
| `t.Run()` subtests | `[Theory]` parameterization |
| `t.Helper()` | N/A (stack traces work differently) |
| `t.Parallel()` | xUnit runs tests in parallel by default |
| `t.Context()` | `CancellationToken` via test fixtures |

**Files:** `FactTests.cs`, `TheoryTests.cs`, `TestFixtures.cs`, `README.md`

#### 3.14 Testify → FluentAssertions

| Go | C# |
|----|-----|
| `assert.Equal(t, expected, actual)` | `actual.Should().Be(expected)` |
| `assert.NoError(t, err)` | `act.Should().NotThrow()` |
| `require.NotNil(t, x)` | `x.Should().NotBeNull()` |
| Test suites | xUnit `IClassFixture<T>` |

**Files:** `FluentAssertionExamples.cs`, `README.md`

#### 3.15 Benchmark → BenchmarkDotNet

| Go | C# |
|----|-----|
| `func BenchmarkXxx(b *testing.B)` | `[Benchmark]` attribute |
| `b.N` loop | Automatic iteration by BenchmarkDotNet |
| `benchstat` | Built-in statistical analysis |
| Memory profiling | `[MemoryDiagnoser]` attribute |

**Files:** `BenchmarkExamples.cs`, `README.md`

#### 3.16 HTTP → HttpClient & Minimal APIs

| Go | C# |
|----|-----|
| `net/http` server | ASP.NET Core minimal APIs |
| `http.HandleFunc` | `app.MapGet()`, `app.MapPost()` |
| `http.Client` | `HttpClient`, `IHttpClientFactory` |
| JSON marshal/unmarshal | `System.Text.Json` serialization |

**Files:** `MinimalApiExample.cs`, `HttpClientExample.cs`, `README.md`

#### 3.17 HTTP Test → WebApplicationFactory

| Go | C# |
|----|-----|
| `httptest.NewRecorder()` | `WebApplicationFactory<T>` in-memory test server |
| `httptest.NewServer()` | `TestServer` |
| Handler unit tests | Integration tests with real middleware pipeline |

**Files:** `IntegrationTestExamples.cs`, `README.md`

#### 3.18 Generics

| Go | C# |
|----|-----|
| `func F[T any](x T)` | `void F<T>(T x)` |
| `comparable` constraint | `where T : IEquatable<T>` |
| Custom constraints | `where T : class`, `where T : struct`, `where T : new()`, etc. |
| Monomorphization (compile-time) | Reified generics (runtime type info preserved) |
| Zero value `var zero T` | `default(T)` or `default` |

**Files:** `GenericMethods.cs`, `GenericClasses.cs`, `Constraints.cs`, `README.md`

#### 3.19 Mocking → Moq / NSubstitute

| Go | C# |
|----|-----|
| Mockery code generation | Moq (reflection-based, no codegen needed) |
| `mock.On("Method").Return()` | `mock.Setup(x => x.Method()).Returns()` |
| `mock.AssertCalled()` | `mock.Verify(x => x.Method())` |

**Files:** `MoqExamples.cs`, `README.md`

#### 3.20 Build Tags → Conditional Compilation

| Go | C# |
|----|-----|
| `//go:build linux` | `#if LINUX`, `RuntimeInformation.IsOSPlatform()` |
| Build tag files | `#if DEBUG`, `#if RELEASE` |
| N/A | `<DefineConstants>` in `.csproj` |

**Files:** `ConditionalCompilation.cs`, `README.md`

### Tasks (Phase 3)

- [ ] Create `internal/Basics/` folder structure (20 topic folders)
- [ ] Create `tests/Basics.Tests/` xUnit project
- [ ] For each topic: write README.md, implementation .cs files, and test files
- [ ] Ensure all tests pass with `dotnet test`

---

## Phase 4 — Module 3: Bank Service (ASP.NET Core)

Build a production-grade layered banking API, mirroring the GoTraining Bank service.

### Layer Mapping

| Go Layer | C# Project | Technology |
|----------|-----------|------------|
| `domain/` (entities + sentinel errors) | `Bank.Domain` | POCOs, records, custom exceptions |
| `repository/` (go-jet + Postgres) | `Bank.Repository` | EF Core + Npgsql |
| `service/` (business logic) | `Bank.Service` | Interfaces + implementations |
| `api/` (Gin router + middleware) | `Bank.Api` | ASP.NET Core controllers + middleware |
| `cli/` (Cobra) | `Bank.Cli` | `System.CommandLine` |
| `config/` (Viper) | `appsettings.json` | `IConfiguration` / `IOptions<T>` |

### Bank.Domain

- `Account.cs` — Account entity (record or class)
- `Transaction.cs` — Transaction entity
- `Transfer.cs` — Transfer entity
- `Exceptions/` — `AccountNotFoundException`, `InsufficientFundsException`

### Bank.Repository

- `IBankRepository.cs` — Repository interface
- `BankDbContext.cs` — EF Core DbContext with entity configurations
- `PostgresBankRepository.cs` — EF Core implementation
- EF Core migrations (or reuse existing SQL migrations)

### Bank.Service

- `IBankService.cs` — Service interface
- `BankService.cs` — Business logic (validates, orchestrates repo calls)

### Bank.Api

- `Program.cs` — App bootstrap, DI registration, middleware pipeline
- `Controllers/AccountController.cs` — `GET /v1/accounts`, `POST /v1/accounts`, `GET /v1/accounts/{id}`
- `Controllers/TransferController.cs` — `GET /v1/transfers`, `POST /v1/transfers`
- `Middleware/AuthMiddleware.cs` — JWT bearer authentication
- `Middleware/LoggingMiddleware.cs` — Request/response logging (Serilog)
- `Middleware/TracingMiddleware.cs` — OpenTelemetry tracing

### Bank.Cli

- `System.CommandLine` with commands: `account create`, `account get`, `transfer create`

### Bank.Tests

- Unit tests for service layer (Moq for repository)
- Integration tests for API (WebApplicationFactory)
- Repository tests (against test Postgres or in-memory)

### Tasks (Phase 4)

- [ ] Create all Bank.* projects and add to solution
- [ ] Implement domain entities and exceptions
- [ ] Set up EF Core DbContext + migrations
- [ ] Implement repository layer
- [ ] Implement service layer
- [ ] Build API controllers + middleware pipeline
- [ ] Wire up DI in Program.cs
- [ ] Build CLI with System.CommandLine
- [ ] Write unit and integration tests
- [ ] Add OpenAPI/Swagger support
- [ ] Add README for each layer

---

## Phase 5 — Module 4: Temporal Orchestration

Port the Temporal order-processing workflow using the official **Temporal .NET SDK**.

### Component Mapping

| Go Component | C# Component | Notes |
|-------------|-------------|-------|
| Workflow functions | `[Workflow]` class + `[WorkflowRun]` method | |
| `workflow.ExecuteActivity()` | `Workflow.ExecuteActivityAsync()` | |
| `workflow.GetSignalChannel()` | `[WorkflowSignal]` handler methods | |
| `workflow.ExecuteChildWorkflow()` | `Workflow.ExecuteChildWorkflowAsync()` | |
| Activities (plain functions) | `[Activity]` methods on a class | |
| Activity options (retry) | `ActivityOptions` with `RetryPolicy` | |
| Worker process | .NET Worker Service + `TemporalWorkerService` | |
| Payload encryption | Custom `IPayloadCodec` implementation | |
| WireMock inventory client | `HttpClient` + `IHttpClientFactory` | |

### Projects

- **Temporal.Domain** — `Order.cs`, `OrderStatus.cs` (enum)
- **Temporal.Workflows** — Workflow classes, activity classes, encryption
- **Temporal.Worker** — Worker Service host
- **Temporal.Client** — Console app to start workflows
- **Temporal.Tests** — Workflow + activity tests using Temporal test environment

### Tasks (Phase 5)

- [ ] Create Temporal.* projects and add to solution
- [ ] Implement order domain model
- [ ] Implement `OrderWorkflow` and `PaymentWorkflow`
- [ ] Implement activities (validate, process, pick, ship, check inventory)
- [ ] Implement payload encryption codec
- [ ] Build worker service
- [ ] Build client console app
- [ ] Write workflow and activity tests
- [ ] Add READMEs for workflows, activities, and order model

---

## Phase 6 — Challenges

### Basics/FixMe

Create intentionally buggy C# code for students to debug:
- Race conditions with shared state
- Deadlocks with improper `async` usage
- Null reference exceptions
- Off-by-one errors
- Improper disposal of resources

### Basics/ImplMe

Create method stubs with `throw new NotImplementedException()` (equivalent to Go's `panic`):
- Implement a generic collection method
- Implement an async pipeline
- Implement a custom middleware

### Bank/Transfer Quest

Same exercise as GoTraining: implement `POST /v1/transfers` end-to-end:
- Add `Transfer` entity to domain
- Add repository methods
- Add service method with validation
- Add API controller endpoint
- Write tests

### Tasks (Phase 6)

- [ ] Create FixMe challenge projects with buggy code + tests
- [ ] Create ImplMe challenge projects with stubs + tests
- [ ] Create Bank Transfer Quest with TODO markers

---

## Phase 7 — Documentation

### Root README.md

- Workshop title and overview
- Prerequisites: .NET 10 SDK, Docker Desktop, IDE (VS Code / Rider / Visual Studio)
- Getting started: `git clone`, `dotnet restore`, `dotnet build`, `dotnet test`, `make infra-up`
- Module structure (4 modules)
- Challenges overview
- Recommended reading

### Per-Module READMEs

Adapt from GoTraining, maintaining the same structure:
1. Title
2. Problem statement
3. Core concepts table
4. Mermaid diagrams
5. C# code examples
6. Common patterns
7. Pitfalls & best practices
8. Running examples (`dotnet test --filter "FullyQualifiedName~TopicName"`)
9. Further reading (Microsoft Learn, C# docs)

### Tasks (Phase 7)

- [ ] Write root README.md
- [ ] Write/adapt README for each Basics topic (20 READMEs)
- [ ] Write/adapt README for each Fundamentals topic (5 READMEs)
- [ ] Write README for each Bank layer (5 READMEs)
- [ ] Write README for each Temporal component (3 READMEs)
- [ ] Write Challenges README

---

## Phase 8 — CI/CD

### GitHub Actions Workflow

```yaml
name: Build & Test
on: [push, pull_request]
jobs:
  build:
    runs-on: ubuntu-latest
    steps:
      - uses: actions/checkout@v4
      - uses: actions/setup-dotnet@v4
        with:
          dotnet-version: '10.0.x'
      - run: dotnet restore
      - run: dotnet build --no-restore
      - run: dotnet format --verify-no-changes
      - run: dotnet test --no-build
```

### Tasks (Phase 8)

- [ ] Create `.github/workflows/build.yml`
- [ ] Verify CI passes on initial push

---

## Concept Mapping Reference

Quick-reference table for Go → C# concept equivalents used throughout the project.

| Go Concept | C# Equivalent |
|-----------|--------------|
| `go.mod` / `go.sum` | `.csproj` / `.sln` / NuGet |
| `cmd/` | Executable projects |
| `internal/` | `internal` access modifier |
| `pkg/` | Shared class libraries |
| Goroutines | `Task.Run()`, `async/await` |
| Channels | `Channel<T>` |
| `sync.WaitGroup` | `Task.WhenAll()` |
| `sync.Mutex` | `lock` / `SemaphoreSlim` |
| `context.Context` | `CancellationToken` |
| `error` interface | `Exception` hierarchy |
| `fmt.Errorf("%w")` | Inner exceptions |
| Implicit interfaces | Explicit `interface` + `: IInterface` |
| Struct embedding | Inheritance / composition |
| `init()` | Static constructors / `[ModuleInitializer]` |
| Value/pointer receivers | Instance/extension methods |
| Table-driven tests | `[Theory]` + `[InlineData]` |
| `testify` | FluentAssertions |
| `httptest` | `WebApplicationFactory` |
| Mockery | Moq / NSubstitute |
| Gin | ASP.NET Core (controllers or minimal APIs) |
| go-jet | Entity Framework Core |
| Cobra | System.CommandLine |
| Viper | IConfiguration / IOptions |
| slog | ILogger / Serilog |
| Build tags | `#if` directives |
| `go:embed` | Embedded resources |
| Benchmarks (`testing.B`) | BenchmarkDotNet |

---

## NuGet Packages

| Purpose | Package |
|---------|---------|
| Web API framework | `Microsoft.AspNetCore.App` (built-in) |
| ORM | `Microsoft.EntityFrameworkCore` |
| Postgres provider | `Npgsql.EntityFrameworkCore.PostgreSQL` |
| JWT auth | `Microsoft.AspNetCore.Authentication.JwtBearer` |
| CLI framework | `System.CommandLine` |
| Structured logging | `Serilog.AspNetCore` |
| Tracing | `OpenTelemetry.Extensions.Hosting` |
| Tracing exporter | `OpenTelemetry.Exporter.OtlpProtocol` |
| Unit testing | `xunit`, `Microsoft.NET.Test.Sdk` |
| Assertions | `FluentAssertions` |
| Mocking | `Moq` |
| Benchmarking | `BenchmarkDotNet` |
| HTTP test | `Microsoft.AspNetCore.Mvc.Testing` |
| Temporal SDK | `Temporalio.Client`, `Temporalio.Worker` |
| Temporal hosting | `Temporalio.Extensions.Hosting` |
| JSON | `System.Text.Json` (built-in) |
| UUID | `System.Guid` (built-in) |
| Decimal | `decimal` (built-in) |

---

## Execution Order Summary

| Phase | Effort | Description |
|-------|--------|-------------|
| 0 | Small | Scaffold solution, projects, folder structure |
| 1 | Small | Docker, Makefile, Dockerfiles, copy wiremock/migration |
| 2 | Small | Port Fundamentals docs (mostly copy + adapt) |
| 3 | **Large** | 20 Basics topic modules (code + tests + READMEs) |
| 4 | **Large** | Bank service (5 projects, full stack) |
| 5 | Medium | Temporal workflows (4 projects) |
| 6 | Medium | Challenges (FixMe, ImplMe, Transfer Quest) |
| 7 | Medium | All READMEs and documentation |
| 8 | Small | CI/CD workflow |
