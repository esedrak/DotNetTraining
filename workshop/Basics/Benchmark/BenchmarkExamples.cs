using System.Text;
using BenchmarkDotNet.Attributes;

namespace DotNetTraining.Basics.Benchmark;

/// <summary>
/// Benchmarks comparing different string concatenation approaches.
/// BenchmarkDotNet is the .NET equivalent of Go's testing.B benchmarks.
///
/// To run: dotnet run --project tests/Basics.Tests -c Release -- --filter "*String*"
/// </summary>
[MemoryDiagnoser]       // Tracks heap allocations — equivalent to b.ReportAllocs()
[SimpleJob]
public class StringConcatBenchmarks
{
    private const int N = 1_000;

    [Benchmark(Baseline = true)]
    public string StringConcat()
    {
        var result = "";
        for (int i = 0; i < N; i++)
        {
            result += i.ToString();
        }

        return result;
    }

    [Benchmark]
    public string StringBuilderAppend()
    {
        var sb = new StringBuilder(N * 4);
        for (int i = 0; i < N; i++)
        {
            sb.Append(i);
        }

        return sb.ToString();
    }

    [Benchmark]
    public string StringCreate()
        => string.Create(N * 4, N, (span, count) =>
        {
            int pos = 0;
            for (int i = 0; i < count; i++)
            {
                i.TryFormat(span[pos..], out int written);
                pos += written;
            }
        });
}

/// <summary>
/// Benchmarks comparing collection lookup patterns.
/// Demonstrates [Params] for running with different input sizes.
/// </summary>
[MemoryDiagnoser]
public class CollectionLookupBenchmarks
{
    [Params(10, 100, 1_000)]
    public int N { get; set; }

    private List<int> _list = [];
    private HashSet<int> _hashSet = [];
    private Dictionary<int, int> _dict = [];

    [GlobalSetup]
    public void Setup()
    {
        _list = Enumerable.Range(0, N).ToList();
        _hashSet = [.. _list];
        _dict = _list.ToDictionary(x => x);
    }

    [Benchmark(Baseline = true)]
    public bool ListContains() => _list.Contains(N / 2);

    [Benchmark]
    public bool HashSetContains() => _hashSet.Contains(N / 2);

    [Benchmark]
    public bool DictionaryContains() => _dict.ContainsKey(N / 2);
}
