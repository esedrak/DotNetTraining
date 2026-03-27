# Benchmarking with BenchmarkDotNet

**BenchmarkDotNet** is the standard .NET micro-benchmarking library. It handles warmup iterations, statistical analysis (mean, standard deviation, confidence intervals), and memory allocation profiling automatically -- giving you reliable, reproducible results.

---

## 1. Core Concepts

| Concept | Description |
| :--- | :--- |
| **`[Benchmark]`** | Marks a method as a benchmark target |
| **`[MemoryDiagnoser]`** | Tracks GC allocations and memory per operation |
| **`[SimpleJob]`** | Configures the benchmark runtime and iteration settings |
| **`[Baseline = true]`** | Marks a benchmark as the baseline for relative comparison |
| **`BenchmarkRunner.Run<T>()`** | Executes all benchmarks in the given class |

---

## 2. Setup

```xml
<!-- tests/Basics.Tests/Basics.Tests.csproj -->
<PackageReference Include="BenchmarkDotNet" Version="0.14.*" />
```

---

## 3. Example

```csharp
// Run benchmarks from your Main method or a dedicated project
BenchmarkRunner.Run<StringConcatBenchmarks>();
```

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

## 4. Pitfalls & Best Practices

1. Always run in **Release** mode (`-c Release`) — Debug builds give misleading results.
2. Use `[MemoryDiagnoser]` to spot hidden allocations.
3. Use `[Baseline = true]` to compare relative performance.
4. Don't benchmark in unit test projects — keep benchmarks in a dedicated project or use `[BenchmarkSwitcher]`.

---

## 5. Running the Examples

```bash
dotnet run --project tests/Basics.Tests -c Release -- --filter "*Benchmark*"
```

---

## 6. Further Reading

- [BenchmarkDotNet docs](https://benchmarkdotnet.org/articles/overview.html)
- [Diagnosers](https://benchmarkdotnet.org/articles/configs/diagnosers.html)

---

<details>
<summary>Coming from Go?</summary>

| Go | C# |
|---|---|
| `func BenchmarkXxx(b *testing.B)` | `[Benchmark]` method on a class |
| `for i := 0; i < b.N; i++` | Handled automatically by BenchmarkDotNet |
| `b.ReportAllocs()` | `[MemoryDiagnoser]` attribute on class |
| `benchstat` | Built-in statistical output (mean, stddev, etc.) |
| `b.SetBytes(n)` | `[Benchmark(OperationsPerInvoke = n)]` |

</details>

## Your Next Step
With your code measured and optimised, you're ready to leverage .NET's powerful async/await model for concurrent operations.
Explore **[Concurrency](../Concurrency/README.md)** to learn about `async/await`, `Task`, and `Channel<T>`.
