# Module 2 — C# Language Basics

This module covers 23 core C# / .NET concepts organized by theme. Every topic has:

1. `README.md` — concept explanation with Mermaid diagrams
2. `.cs` implementation files — working code examples
3. Tests in `tests/Basics.Tests/` — runnable via `dotnet test`

---

## Topics

### Types & Memory
- **[Value & Reference Types](./ValueAndReferenceTypes/)** — struct vs class, `ref`/`out`/`in`, `Span<T>`
- **[Parameters](./Parameters/)** — `params`, optional params, named args, parameter modifiers
- **[Entities](./Entity/)** — `class`, `record`, `struct`, `record struct`, `init`/`required`
- **[Nullable Reference Types](./NullableReferenceTypes/)** — `?` annotations, `?.`, `??`, `??=`, null guards

### OOP & Patterns
- **[Receivers](./Receivers/)** — Instance methods, extension methods, C# 14 `extension` blocks
- **[Interfaces](./Interface/)** — Explicit `: IInterface`, DI container, default methods
- **[Type Assertions & Casting](./Casting/)** — `is`, `as`, switch expression, pattern matching
- **[Composition & Inheritance](./CompositionAndInheritance/)** — `<EmbeddedResource>`, base classes, decorator pattern
- **[Generics](./Generics/)** — `void F<T>`, `where T : constraint`, `default(T)`

### Error Handling & Resources
- **[Error Handling](./ErrorHandling/)** — Exceptions, `catch when`, `Result<T>` pattern
- **[Disposable & Resource Management](./Disposable/)** — `IDisposable`, `using`, `IAsyncDisposable`, `await using`

### Code Organisation
- **[Project Layout](./Layout/)** — `.sln`, `.csproj`, namespaces, `internal` modifier
- **[Initialization](./Initialization/)** — Static constructors, `[ModuleInitializer]`, `Lazy<T>`

### Data & Queries
- **[LINQ](./Linq/)** — `Where`, `Select`, `GroupBy`, deferred execution, method vs query syntax

### Testing
- **[Testing](./Testing/)** — `[Fact]`, `[Theory]`, `[InlineData]`, `IClassFixture<T>`
- **[FluentAssertions](./FluentAssertions/)** — `.Should().Be()`, `.Should().Throw<T>()`
- **[Mocking](./Mocking/)** — Moq — `Mock<T>`, `Setup`, `Verify`, argument matchers

### HTTP Services
- **[HTTP Client & Server](./Http/)** — Minimal APIs, `HttpClient`, `IHttpClientFactory`
- **[HTTP Testing](./HttpTest/)** — `WebApplicationFactory<T>`, integration tests
- **[Benchmark](./Benchmark/)** — BenchmarkDotNet, `[Benchmark]`, `[MemoryDiagnoser]`

### Concurrency & Context
- **[Concurrency](./Concurrency/)** — `async/await`, `Task`, `Channel<T>`, `lock`, `Interlocked`
- **[Context](./Context/)** — `CancellationToken`, `CancellationTokenSource`, `AsyncLocal<T>`

### Compilation & Platform
- **[Conditional Compilation](./ConditionalCompilation/)** — `#if DEBUG`, `<DefineConstants>`, `RuntimeInformation`

---

## Running Tests

```bash
# All Basics tests
dotnet test tests/Basics.Tests

# Single topic
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Linq"
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Concurrency"

# With verbose output
dotnet test tests/Basics.Tests --logger "console;verbosity=detailed"
```

## Your Exploration Journey

Ready to master the fundamentals of C#? We've designed a guided path to take you from the basic types to advanced compilation techniques.

Start your journey with the building blocks: **[Value & Reference Types](./ValueAndReferenceTypes/)**.

---
[← Back to Main README](../../README.md)
