# 🏷️ Conditional Compilation in C#

Go's `//go:build` tags have two equivalents in C#: **preprocessor directives** (`#if`) for compile-time conditions, and **`RuntimeInformation`** for runtime OS/platform detection.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`#if SYMBOL`** | Include/exclude code at compile time based on defined symbols |
| **`#if DEBUG`** | Built-in symbol — defined in Debug builds |
| **`#if RELEASE`** | Built-in symbol — defined in Release builds |
| **`<DefineConstants>`** | Define custom symbols in `.csproj` |
| **`RuntimeInformation`** | Runtime OS/platform detection (safer than compile-time for most cases) |

---

## 2. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `//go:build linux` | `#if LINUX` + `<DefineConstants>LINUX</DefineConstants>` |
| `//go:build !windows` | `#if !WINDOWS` |
| `//go:build debug` | `#if DEBUG` |
| `runtime.GOOS == "linux"` | `RuntimeInformation.IsOSPlatform(OSPlatform.Linux)` |
| Separate build tag files | All in one file with `#if` blocks, or partial classes |

---

## 3. Examples

### Preprocessor directives

```csharp
#if DEBUG
    Console.WriteLine("Debug mode — verbose logging enabled");
    services.AddSingleton<ILogger, VerboseLogger>();
#else
    services.AddSingleton<ILogger, SilentLogger>();
#endif

// Conditional method — only compiled in Debug
[Conditional("DEBUG")]
public static void Assert(bool condition, string message)
{
    if (!condition) throw new InvalidOperationException(message);
}
```

### Custom symbols in `.csproj`

```xml
<PropertyGroup Condition="'$(EnableFeatureX)' == 'true'">
  <DefineConstants>$(DefineConstants);FEATURE_X</DefineConstants>
</PropertyGroup>
```

```csharp
#if FEATURE_X
    app.MapGet("/experimental", ExperimentalHandler);
#endif
```

### Runtime OS detection (preferred for platform branching)

```csharp
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    // Windows-specific code
else if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
    // Linux-specific code
```

---

## ⚠️ Pitfalls & Best Practices

1. Prefer **runtime checks** (`RuntimeInformation`) over compile-time `#if` for platform differences — easier to test.
2. `#if DEBUG` code is excluded from Release builds — use `[Conditional("DEBUG")]` for helper methods.
3. Avoid complex `#if` logic — it makes code hard to read and test. Extract to separate classes instead.

---

## 📚 Further Reading

- [Preprocessor directives (C# docs)](https://learn.microsoft.com/en-us/dotnet/csharp/language-reference/preprocessor-directives)
- [RuntimeInformation](https://learn.microsoft.com/en-us/dotnet/api/system.runtime.interopservices.runtimeinformation)
