# ✅ FluentAssertions in C#

**FluentAssertions** is the C# equivalent of Go's `testify/assert`. It provides a readable, chainable assertion syntax with rich failure messages.

---

## 1. Go → C# Mapping

| Go (testify) | C# (FluentAssertions) |
| :--- | :--- |
| `assert.Equal(t, expected, actual)` | `actual.Should().Be(expected)` |
| `assert.NotEqual(t, a, b)` | `actual.Should().NotBe(unexpected)` |
| `assert.Nil(t, x)` | `x.Should().BeNull()` |
| `assert.NotNil(t, x)` | `x.Should().NotBeNull()` |
| `assert.True(t, cond)` | `cond.Should().BeTrue()` |
| `assert.False(t, cond)` | `cond.Should().BeFalse()` |
| `assert.NoError(t, err)` | `act.Should().NotThrow()` |
| `assert.ErrorIs(t, err, target)` | `act.Should().Throw<SpecificException>()` |
| `assert.Contains(t, slice, elem)` | `collection.Should().Contain(elem)` |
| `assert.Len(t, slice, n)` | `collection.Should().HaveCount(n)` |
| `require.Equal(...)` (stops test) | FluentAssertions throws immediately by default |

---

## 2. Examples

```csharp
// Basic equality
result.Should().Be(42);
name.Should().Be("Alice");

// Collections
list.Should().HaveCount(3);
list.Should().Contain("expected");
list.Should().BeInAscendingOrder();
list.Should().NotBeEmpty();

// Strings
message.Should().StartWith("Hello");
message.Should().Contain("world");
message.Should().MatchRegex(@"^\d+$");

// Exceptions
var act = () => service.GetAccount(-1);
act.Should().Throw<ArgumentException>()
   .WithMessage("*must be positive*");

// Async exceptions
var actAsync = async () => await service.GetAccountAsync(-1);
await actAsync.Should().ThrowAsync<NotFoundException>();

// Objects
account.Should().NotBeNull();
account.Should().BeEquivalentTo(expected);  // deep structural equality

// Nullable
nullable.Should().HaveValue().And.Be(42);
nullableEmpty.Should().NotHaveValue();
```

---

## 3. `BeEquivalentTo` — deep structural comparison

```csharp
// Compares all public properties recursively — like reflect.DeepEqual in Go
actual.Should().BeEquivalentTo(expected, options =>
    options.Excluding(x => x.Id)  // ignore auto-generated Id
           .Excluding(x => x.CreatedAt));
```

---

## ⚠️ Pitfalls & Best Practices

1. Prefer `Should().Be()` for exact equality and `Should().BeEquivalentTo()` for structural/deep comparison.
2. `Should().Throw<T>()` only works with synchronous code; use `Should().ThrowAsync<T>()` for async.
3. FluentAssertions gives much better failure messages than xUnit's built-in `Assert.*` — always prefer it.

---

## 🏃 Running the Examples

```bash
dotnet test tests/Basics.Tests --filter "FullyQualifiedName~Testify"
```

---

## 📚 Further Reading

- [FluentAssertions docs](https://fluentassertions.com/introduction)

## Your Next Step
After writing expressive assertions, the next step is to isolate your dependencies so your tests remain fast and focused.
Explore **[Mocking with Moq](../Mocking/README.md)** to learn how to generate type-safe mocks for your interfaces.
