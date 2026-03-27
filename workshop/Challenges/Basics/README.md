# Day 1 Challenges: Detective Briefs

Short exercises covering core C# building blocks, concurrency pitfalls, and idiomatic patterns. Each challenge is a self-contained mystery.

---

## Challenge Types

### FixMe — Find and fix the bug

Buggy code is provided. Your task is to identify the root cause and fix it. Each bug represents a real-world C# pitfall.

### ImplMe — Implement the stub

You'll find `throw new NotImplementedException()` methods. Your task is to implement the method body to make the tests pass.

---

## FixMe Challenges

| # | Class | C# Focus Area | Bug |
|---|---|---|---|
| 1 | `RaceCounter` | Concurrency / `Interlocked` | Non-atomic `_count++` across tasks |
| 2 | `DeadlockExample` | Async / `.Result` trap | Blocking on async with `.Result` |
| 3 | `NullBug` | Nullable reference types | Dereference without null/empty check |
| 4 | `OffByOne` | Loop bounds | Last element skipped (`< Length - 1`) |
| 5 | `ResourceLeak` | `IDisposable` / `HttpClient` | Socket leak from undisposed client |
| 6 | `StructMutationBug` | Value vs reference types | Struct copy semantics silently lose mutations |
| 7 | `DisposableLeakBug` | `IDisposable` / `using` | Resources created in loop but never disposed |
| 8 | `AsyncVoidBug` | `async void` | Unobservable exceptions from fire-and-forget |

## ImplMe Challenges

| # | Class | C# Focus Area | Task |
|---|---|---|---|
| 1 | `CollectionExtensions` | Generics / IEnumerable | Implement `Chunk`, `MostFrequent`, `Flatten` |
| 2 | `AsyncPipeline` | Async / SemaphoreSlim | Implement concurrent processing with retry |
| 3 | `CorrelationIdMiddleware` | ASP.NET Core middleware | Add correlation ID to request/response headers |
| 4 | `LinqChallenge` | LINQ queries | Filter, group, and aggregate bank transactions |
| 5 | `ResultExtensions` | Result pattern / error handling | Implement `TryCatch` and `Map` for `Result<T>` |

---

## Running Tests

```bash
# All challenge tests
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Challenge"

# FixMe only
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~ChallengeFixMe"

# ImplMe only
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~ChallengeImplMe"
```

> **Note:** ImplMe tests will fail until you implement the stubs. FixMe tests document the buggy behavior — after fixing a bug, update the test to verify the correct behavior.

---

[← Back to Challenges](../README.md)
