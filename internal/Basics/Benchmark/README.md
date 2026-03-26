# 📊 Benchmarking with BenchmarkDotNet

**BenchmarkDotNet** is the .NET equivalent of Go's `testing.B` benchmarks. It handles warmup, statistical analysis, and memory profiling automatically.

---

## 1. Go → C# Mapping

| Go | C# |
| :--- | :--- |
| `func BenchmarkXxx(b *testing.B)` | `[Benchmark]` method on a class |
| `for i := 0; i < b.N; i++` | Handled automatically by BenchmarkDotNet |
| `b.ReportAllocs()` | `[MemoryDiagnoser]` attribute on class |
| `benchstat` | Built-in statistical output (mean, stddev, etc.) |
| `b.SetBytes(n)` | `[Benchmark(OperationsPerInvoke = n)]` |

---

## 2. Setup

```xml
<!-- tests/Basics.Tests/Basics.Tests.csproj -->
<PackageReference Include="BenchmarkDotNet" Version="0.14.*" />
```

---

## 3. Example

```csharp
[MemoryDiagnoser]
[SimpleJob(RuntimeMoniker.Net90)]
public class StringConcatBenchmarks
{
    private const int N = 1000;

    [Benchmark(Baseline = true)]
    public string StringConcat()
    {
        var result = "";
        for (int i = 0; i < N; i++)
            result += i.ToString();
        return result;
    }

    [Benchmark]
    public string StringBuilder()
    {
        var sb = new System.Text.StringBuilder();
        for (int i = 0; i < N; i++)
            sb.Append(i);
        return sb.ToString();
    }
}
```

### Running benchmarks

```bash
# BenchmarkDotNet must run in Release mode
dotnet run --project tests/Basics.Tests -c Release -- --filter "*StringConcat*"
```

### Sample output

```
| Method        | Mean      | Error    | StdDev   | Gen0    | Allocated |
|-------------- |----------:|---------:|---------:|--------:|----------:|
| StringConcat  | 412.3 μs  | 5.12 μs  | 4.79 μs  | 203.125 | 1254 KB   |
| StringBuilder |   4.8 μs  | 0.09 μs  | 0.08 μs  |  0.9918 |   6.16 KB |
```

---

## ⚠️ Pitfalls & Best Practices

1. Always run in **Release** mode (`-c Release`) — Debug builds give misleading results.
2. Use `[MemoryDiagnoser]` to spot hidden allocations.
3. Use `[Baseline = true]` to compare relative performance.
4. Don't benchmark in unit test projects — keep benchmarks in a dedicated project or use `[BenchmarkSwitcher]`.

---

## 🏃 Running the Examples

```bash
dotnet run --project tests/Basics.Tests -c Release -- --filter "*Benchmark*"
```

---

## 📚 Further Reading

- [BenchmarkDotNet docs](https://benchmarkdotnet.org/articles/overview.html)
- [Diagnosers](https://benchmarkdotnet.org/articles/configs/diagnosers.html)
