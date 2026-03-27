# 🧪 Testing in C# with xUnit

xUnit is the standard test framework for .NET. Its patterns map closely to Go's `testing` package: `[Fact]` ≈ `func TestXxx`, `[Theory]` ≈ table-driven tests.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`[Fact]`** | Single test case (like `func TestXxx(t *testing.T)`) |
| **`[Theory]`** | Parameterized test — runs once per data set |
| **`[InlineData]`** | Inline data for `[Theory]` (like table-driven test rows) |
| **`[MemberData]`** | Data from a static property — for complex objects |
| **`IClassFixture<T>`** | Shared setup/teardown across tests in a class |
| **`IAsyncLifetime`** | Async setup/teardown (like `TestMain` in Go) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `func TestXxx(t *testing.T)` | `[Fact] public void TestXxx()` |
| `t.Error("msg")` / `t.Fatal("msg")` | `Assert.True(condition)` / `Assert.Equal(expected, actual)` |
| Table-driven `tests := []struct{...}` | `[Theory] [InlineData(...)]` |
| `t.Run("name", func(t))` | `[Theory]` runs automatically per data set |
| `t.Parallel()` | xUnit runs tests in parallel by default |
| `TestMain(m *testing.M)` | `IAsyncLifetime` or `IClassFixture<T>` |
| `t.Context()` | Pass `CancellationToken` via fixture |

---

## 3. Examples

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

### Theory — table-driven tests (replaces Go's `for _, tt := range tests`)

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

## ⚠️ Pitfalls & Best Practices

1. Name tests: `MethodName_StateUnderTest_ExpectedBehavior` (clear failures).
2. One logical assertion per test — easier to diagnose failures.
3. Don't share mutable state between tests — each test should be isolated.
4. Use `[Theory]` for multiple inputs — don't copy-paste test methods.

---

## 🏃 Running the Examples

```bash
# All tests
dotnet test tests/Basics.Tests

# Filter by topic
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Testing"

# Verbose
dotnet test tests/Basics.Tests --logger "console;verbosity=detailed"
```

---

## 📚 Further Reading

- [xUnit docs](https://xunit.net/docs/getting-started/netcore/cmdline)
- [Unit testing best practices (.NET)](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-best-practices)

## Your Next Step
Now that you know the basics of testing, you can make your assertions more readable and expressive using FluentAssertions.
Explore **[Testify (FluentAssertions)](../Testify/README.md)** to see how to write fluent, human-readable assertions.
