using DotNetTraining.Basics.Concurrency;
using FluentAssertions;

namespace Basics.Tests;

public class ConcurrencyTests
{
    [Fact]
    public async Task RunConcurrent_ReturnsAllSquaredResults()
    {
        var results = await TaskExamples.RunConcurrentAsync(5);
        results.Should().HaveCount(5);
        results.Should().Contain(0);  // 0*0
        results.Should().Contain(4);  // 2*2
        results.Should().Contain(16); // 4*4
    }

    [Fact]
    public async Task FirstToFinish_ReturnsFastestResult()
    {
        var result = await TaskExamples.FirstToFinishAsync();
        result.Should().Be(2, "the 100ms task wins over the 500ms task");
    }

    [Fact]
    public async Task Channel_ProducesAndConsumes_InOrder()
    {
        var items = await ChannelExamples.ProduceAndConsumeAsync(5);
        items.Should().HaveCount(5);
        items.Should().BeInAscendingOrder();
    }

    [Fact]
    public async Task SafeCounter_HandlesParallelIncrements()
    {
        var counter = new SafeCounter();
        var tasks = Enumerable.Range(0, 100)
            .Select(_ => Task.Run(() => counter.Increment()));
        await Task.WhenAll(tasks);

        counter.Value.Should().Be(100);
    }

    [Fact]
    public async Task AsyncSafeCounter_HandlesParallelIncrements()
    {
        var counter = new AsyncSafeCounter();
        var tasks = Enumerable.Range(0, 50)
            .Select(_ => Task.Run(async () => await counter.IncrementAsync()));
        await Task.WhenAll(tasks);

        counter.Value.Should().Be(50);
    }

    [Fact]
    public void Singleton_ReturnsSameInstance()
    {
        var a = SingletonService.Instance;
        var b = SingletonService.Instance;
        a.Should().BeSameAs(b);
    }
}
