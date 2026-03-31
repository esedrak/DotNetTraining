# Testing in C# with xUnit

xUnit is the standard test framework for .NET. It provides attribute-driven test discovery, built-in parameterized tests, and first-class async support. Tests are regular methods decorated with `[Fact]` (single case) or `[Theory]` (data-driven), and xUnit runs them in parallel by default for fast feedback.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`[Fact]`** | A single test case with no parameters |
| **`[Theory]`** | Parameterized test -- runs once per data set |
| **`[InlineData]`** | Inline data for `[Theory]` -- each attribute becomes a test run |
| **`[MemberData]`** | Data from a static property -- for complex objects that cannot be expressed as attributes |
| **`IClassFixture<T>`** | Shared setup/teardown across all tests in a class |
| **`IAsyncLifetime`** | Async setup and teardown lifecycle hooks |

---

## 2. Examples

### Fact — single test case

```csharp
[Fact]
public void Deposit_IncreasesBalance()
{
    var account = new BankAccount { Owner = "Alice" };
    account.Deposit(100m);
    Assert.Equal(100m, account.Balance);
}
```

### Theory -- parameterized (data-driven) tests

```csharp
[Theory]
[InlineData(1, 2, 3)]
[InlineData(0, 0, 0)]
[InlineData(-1, 1, 0)]
public void Add_ReturnsExpectedSum(int a, int b, int expected)
{
    Assert.Equal(expected, a + b);
}
```

### Async test

```csharp
[Fact]
public async Task GetAccount_ReturnsAccount_WhenExists()
{
    var service = new BankService();
    var result = await service.GetAccountAsync(1);
    Assert.NotNull(result);
}
```

### Class fixture — shared expensive setup

```csharp
public class DatabaseFixture : IAsyncLifetime
{
    public string ConnectionString { get; private set; } = "";

    public async Task InitializeAsync()
    {
        // Start test container, run migrations, etc.
        ConnectionString = "Host=localhost;Database=test;...";
        await Task.CompletedTask;
    }

    public async Task DisposeAsync() => await Task.CompletedTask;
}

public class BankRepositoryTests(DatabaseFixture db) : IClassFixture<DatabaseFixture>
{
    [Fact]
    public async Task CreateAccount_Persists()
    {
        // Use db.ConnectionString
        await Task.CompletedTask;
    }
}
```

---

## Pitfalls & Best Practices

1. Name tests: `MethodName_StateUnderTest_ExpectedBehavior` (clear failures).
2. One logical assertion per test — easier to diagnose failures.
3. Don't share mutable state between tests — each test should be isolated.
4. Use `[Theory]` for multiple inputs — don't copy-paste test methods.

---

## Running the Examples

```bash
# All tests
dotnet test tests/Basics.Tests

# Filter by topic
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Testing"

# Verbose
dotnet test tests/Basics.Tests --logger "console;verbosity=detailed"
```

---

## Further Reading

- [xUnit docs](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Unit testing best practices (.NET)](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

---

## Your Next Step
Now that you know the basics of testing, you can make your assertions more readable and expressive using FluentAssertions.
Explore **[FluentAssertions](../FluentAssertions/README.md)** to see how to write fluent, human-readable assertions.

> Note: Advanced testing styles, including full ASP.NET Core integration tests, are covered later in the Challenges module. For an early preview, see **[AccountControllerIntegrationTests](../../../tests/Bank.Tests/Controllers/AccountControllerIntegrationTests.cs)**
