# Module 2 — C# Language Basics

This module covers 20 core C# / .NET concepts, each mapping to an equivalent from the GoTraining workshop. Every topic has:

1. `README.md` — concept explanation, Go→C# mapping table, Mermaid diagrams
2. `.cs` implementation files — working code examples
3. Tests in `tests/Basics.Tests/` — runnable via `dotnet test`

---

## Topics

### The Basics & The Mental Shift
- **[Pointers](./Pointers/)** — `ref`/`out`/`in`, value vs reference types, `Span<T>`
- **[Parameters](./Parameters/)** — `params`, `ref`/`out`/`in`, optional params, named args
- **[Error Handling](./ErrorHandling/)** — Exceptions, `catch when`, `Result<T>` pattern

### Defining Data & Behaviour
- **[Entities](./Entity/)** — `class`, `record`, `struct`, `record struct`, `init`/`required`
- **[Receivers](./Receivers/)** — Instance methods, extension methods, C# 14 `extension` blocks
- **[Interfaces](./Interface/)** — Explicit `: IInterface`, DI container, default methods
- **[Type Assertions & Casting](./Casting/)** — `is`, `as`, switch expression, pattern matching
- **[Embedding](./Embedding/)** — `<EmbeddedResource>`, inheritance, composition

### Code Organisation
- **[Package Layout](./Layout/)** — `.sln`, `.csproj`, namespaces, `internal` modifier
- **[Init](./Init/)** — Static constructors, `[ModuleInitializer]`, `Lazy<T>`

### Testing
- **[Testing](./Testing/)** — `[Fact]`, `[Theory]`, `[InlineData]`, `IClassFixture<T>`
- **[Testify](./Testify/)** — FluentAssertions — `.Should().Be()`, `.Should().Throw<T>()`
- **[Mocking](./Mocking/)** — Moq — `Mock<T>`, `Setup`, `Verify`, argument matchers

### HTTP Services
- **[HTTP Client & Server](./Http/)** — Minimal APIs, `HttpClient`, `IHttpClientFactory`
- **[HTTP Testing](./HttpTest/)** — `WebApplicationFactory<T>`, integration tests
- **[Benchmark](./Benchmark/)** — BenchmarkDotNet, `[Benchmark]`, `[MemoryDiagnoser]`

### Concurrency & Context
- **[Concurrency](./Concurrency/)** — `async/await`, `Task`, `Channel<T>`, `lock`, `Interlocked`
- **[Context](./Context/)** — `CancellationToken`, `CancellationTokenSource`, `AsyncLocal<T>`

### Advanced Features
- **[Generics](./Generics/)** — `void F<T>`, `where T : constraint`, `default(T)`
- **[Build Tags](./BuildTags/)** — `#if DEBUG`, `<DefineConstants>`, `RuntimeInformation`

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

## Your Exploration Journey

Ready to master the fundamentals of C#? We've designed a guided path to take you from the basic types to advanced compilation techniques.

Start your journey with the building blocks: **[Pointers](./Pointers/)**.

---
[← Back to Main README](../../README.md)
