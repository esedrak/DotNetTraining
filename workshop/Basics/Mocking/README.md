# Mocking with Moq in C#

**Moq** is the most widely used mocking library in .NET. It generates mock implementations of interfaces at runtime using reflection -- no separate code generation step needed. The API is type-safe and lambda-based, giving you compile-time checking of your mock setups.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| `new Mock<IInterface>()` | Create a mock for any interface |
| `.Setup(x => x.Method()).Returns(val)` | Configure what a method returns when called |
| `.Setup(x => x.Method()).ThrowsAsync(ex)` | Configure a method to throw an exception |
| `.Verify(x => x.Method(), Times.Once())` | Assert a method was called exactly once |
| `.Verify(x => x.Method(), Times.Never())` | Assert a method was never called |
| `.VerifyAll()` | Assert all setups were invoked |
| `It.IsAny<T>()`, `It.Is<T>(pred)` | Argument matchers for flexible setup/verification |

---

## 2. Setup and Usage

### Basic mock

```csharp
var mockRepo = new Mock<IBankRepository>();

// Setup — return value
mockRepo
    .Setup(r => r.GetAccountAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(new Account(Guid.NewGuid(), "Alice", 100m));

// Inject into service under test
var service = new BankService(mockRepo.Object);
var account = await service.GetAccountAsync(someId);

// Verify it was called
mockRepo.Verify(
    r => r.GetAccountAsync(someId, It.IsAny<CancellationToken>()),
    Times.Once());
```

### Mock throws an exception

```csharp
mockRepo
    .Setup(r => r.GetAccountAsync(missingId, It.IsAny<CancellationToken>()))
    .ThrowsAsync(new NotFoundException("Account", missingId));
```

### Callback — inspect arguments passed to the mock

```csharp
Account? capturedAccount = null;

mockRepo
    .Setup(r => r.AddAsync(It.IsAny<Account>(), It.IsAny<CancellationToken>()))
    .Callback<Account, CancellationToken>((acc, _) => capturedAccount = acc)
    .Returns(Task.CompletedTask);
```

---

## 3. `MockBehavior.Strict` vs default

```csharp
// Default (Loose): unset calls return default values — no errors
var loose = new Mock<IRepo>();

// Strict: unset calls throw MockException — ensures all calls are configured
var strict = new Mock<IRepo>(MockBehavior.Strict);
```

---

## Pitfalls & Best Practices

1. Only mock things you own — don't mock third-party types you can't control.
2. Use `It.IsAny<T>()` for `CancellationToken` parameters — you rarely care about the specific token in unit tests.
3. Call `mock.VerifyAll()` at the end of tests when using `MockBehavior.Strict`.
4. If a method is called in the constructor, set up the mock *before* constructing the SUT.

---

## Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Mocking"
```

---

## Further Reading

- [Moq quickstart](https://github.com/devlooped/moq/wiki/Quickstart)
- [Unit testing with mock objects](https://learn.microsoft.com/en-us/dotnet/core/testing/unit-testing-with-mocks)

---

## Your Next Step
With your code fully testable, you're ready to expose it to the world through a production-ready HTTP API.
Explore **[HTTP Client & Server](../Http/README.md)** to learn how to build and consume web services with ASP.NET Core.
