using DotNetTraining.Basics.AsyncStreams;
using FluentAssertions;

namespace Basics.Tests;

public class AsyncStreamsTests
{
    [Fact]
    public async Task GenerateAsync_ProducesExpectedCount()
    {
        var result = await AsyncStreamExamples.ToListAsync(
            AsyncStreamExamples.GenerateAsync(5));

        result.Should().HaveCount(5);
    }

    [Fact]
    public async Task GenerateAsync_ProducesExpectedValues()
    {
        var result = await AsyncStreamExamples.ToListAsync(
            AsyncStreamExamples.GenerateAsync(3));

        result.Should().Equal(0, 1, 2);
    }

    [Fact]
    public async Task SelectAsync_TransformsElements()
    {
        var source = AsyncStreamExamples.GenerateAsync(3);
        var transformed = AsyncStreamExamples.SelectAsync(source, x => x * 2);

        var result = await AsyncStreamExamples.ToListAsync(transformed);

        result.Should().Equal(0, 2, 4);
    }

    [Fact]
    public async Task WhereAsync_FiltersElements()
    {
        var source = AsyncStreamExamples.GenerateAsync(5);
        var evens = AsyncStreamExamples.WhereAsync(source, x => x % 2 == 0);

        var result = await AsyncStreamExamples.ToListAsync(evens);

        result.Should().Equal(0, 2, 4);
    }

    [Fact]
    public async Task BatchAsync_GroupsIntoChunks()
    {
        var source = AsyncStreamExamples.GenerateAsync(5);
        var batches = await AsyncStreamExamples.ToListAsync(
            AsyncStreamExamples.BatchAsync(source, size: 2));

        batches.Should().HaveCount(3);
        batches[0].Should().HaveCount(2);
        batches[1].Should().HaveCount(2);
        batches[2].Should().HaveCount(1);
    }

    [Fact]
    public async Task GenerateAsync_RespectsCancel()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var act = async () =>
        {
            await foreach (var _ in AsyncStreamExamples.GenerateAsync(100, ct: cts.Token))
            {
                // should throw before producing anything
            }
        };

        await act.Should().ThrowAsync<OperationCanceledException>();
    }

    [Fact]
    public async Task FileLineReader_ReadsAllLines()
    {
        using var reader = new StringReader("line1\nline2\nline3");

        var lines = await AsyncStreamExamples.ToListAsync(
            FileLineReader.ReadLinesAsync(reader));

        lines.Should().Equal("line1", "line2", "line3");
    }
}
