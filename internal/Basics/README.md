# Module 2 — C# Language Basics

This module covers 20 core C# / .NET concepts, each mapping to an equivalent from the GoTraining workshop. Every topic has:

1. `README.md` — concept explanation, Go→C# mapping table, Mermaid diagrams
2. `.cs` implementation files — working code examples
3. Tests in `tests/Basics.Tests/` — runnable via `dotnet test`

---

## Topics

| # | Folder | Go Equivalent | Key C# Concepts |
|---|--------|---------------|-----------------|
| 1 | [Pointers](./Pointers/) | `*T`, `&x`, `*p` | `ref`/`out`/`in`, value vs reference types, `Span<T>` |
| 2 | [Casting](./Casting/) | `x.(T)`, type switch | `is`, `as`, switch expression, pattern matching |
| 3 | [Parameters](./Parameters/) | `...T`, `*T` | `params`, `ref`/`out`/`in`, optional params, named args |
| 4 | [Entity](./Entity/) | `type X struct {}` | `class`, `record`, `struct`, `record struct`, `init`/`required` |
| 5 | [Layout](./Layout/) | `cmd/`, `internal/`, `pkg/` | `.sln`, `.csproj`, namespaces, `internal` modifier |
| 6 | [Embedding](./Embedding/) | `//go:embed`, struct embedding | `<EmbeddedResource>`, inheritance, composition |
| 7 | [Receivers](./Receivers/) | Value/pointer receivers | Instance methods, extension methods, C# 14 `extension` blocks |
| 8 | [Init](./Init/) | `func init()` | Static constructors, `[ModuleInitializer]`, `Lazy<T>` |
| 9 | [ErrorHandling](./ErrorHandling/) | `error`, `errors.Is/As` | Exceptions, `catch when`, `Result<T>` pattern |
| 10 | [Interface](./Interface/) | Implicit interfaces | Explicit `: IInterface`, DI container, default methods |
| 11 | [Concurrency](./Concurrency/) | goroutines, channels, `sync.*` | `async/await`, `Task`, `Channel<T>`, `lock`, `Interlocked` |
| 12 | [Context](./Context/) | `context.Context` | `CancellationToken`, `CancellationTokenSource`, `AsyncLocal<T>` |
| 13 | [Testing](./Testing/) | `func TestXxx(t *testing.T)` | `[Fact]`, `[Theory]`, `[InlineData]`, `IClassFixture<T>` |
| 14 | [Testify](./Testify/) | `testify/assert` | FluentAssertions — `.Should().Be()`, `.Should().Throw<T>()` |
| 15 | [Benchmark](./Benchmark/) | `testing.B` | BenchmarkDotNet, `[Benchmark]`, `[MemoryDiagnoser]` |
| 16 | [Http](./Http/) | `net/http` server + client | Minimal APIs, `HttpClient`, `IHttpClientFactory` |
| 17 | [HttpTest](./HttpTest/) | `httptest.NewRecorder()` | `WebApplicationFactory<T>`, integration tests |
| 18 | [Generics](./Generics/) | `func F[T any]`, constraints | `void F<T>`, `where T : constraint`, `default(T)` |
| 19 | [Mocking](./Mocking/) | Mockery | Moq — `Mock<T>`, `Setup`, `Verify`, argument matchers |
| 20 | [BuildTags](./BuildTags/) | `//go:build linux` | `#if DEBUG`, `<DefineConstants>`, `RuntimeInformation` |

---

## Running Tests

```bash
# All Basics tests
dotnet test tests/Basics.Tests

# Single topic
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Pointers"
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Concurrency"

# With verbose output
dotnet test tests/Basics.Tests --logger "console;verbosity=detailed"
```

---

## Template

See [TEMPLATE.md](../../GoTraining/internal/basics/TEMPLATE.md) in the original GoTraining repo for the documentation structure used here.
