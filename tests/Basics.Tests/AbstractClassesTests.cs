using DotNetTraining.Basics.AbstractClasses;
using FluentAssertions;

namespace Basics.Tests;

public class AbstractClassesTests
{
    // ── Circle ────────────────────────────────────────────────────────────────

    [Fact]
    public void Circle_Area_IsCorrect()
    {
        var circle = new Circle(5);
        circle.Area().Should().BeApproximately(Math.PI * 25, 0.001);
    }

    [Fact]
    public void Circle_Describe_UsesBaseImplementation()
    {
        var circle = new Circle(5);
        circle.Describe().Should().Contain("Circle");
    }

    // ── Rectangle ─────────────────────────────────────────────────────────────

    [Fact]
    public void Rectangle_Area_IsCorrect()
    {
        var rect = new Rectangle(4, 3);
        rect.Area().Should().Be(12);
    }

    [Fact]
    public void Rectangle_Perimeter_IsCorrect()
    {
        var rect = new Rectangle(4, 3);
        rect.Perimeter().Should().Be(14);
    }

    [Fact]
    public void Rectangle_Describe_IsOverridden()
    {
        var rect = new Rectangle(4, 3);
        // Rectangle overrides Describe() with its own format
        rect.Describe().Should().Contain("Rectangle").And.Contain("4");
    }

    // ── Square ────────────────────────────────────────────────────────────────

    [Fact]
    public void Square_Area_IsCorrect()
    {
        var square = new Square(5);
        square.Area().Should().Be(25);
    }

    [Fact]
    public void Square_IsSealed()
    {
        typeof(Square).IsSealed.Should().BeTrue("sealed prevents further subclassing");
    }

    // ── IsLargerThan (non-virtual concrete method) ────────────────────────────

    [Fact]
    public void Shape_IsLargerThan_ComparesByArea()
    {
        Shape big = new Circle(10);
        Shape small = new Circle(5);

        big.IsLargerThan(small).Should().BeTrue();
        small.IsLargerThan(big).Should().BeFalse();
    }

    // ── Template Method Pattern ───────────────────────────────────────────────

    [Fact]
    public async Task UpperCaseCsvExporter_ExportsCorrectly()
    {
        var exporter = new UpperCaseCsvExporter();
        var result = await exporter.ExportAsync(["hello", "world"]);

        result.Should().Be("HELLO,WORLD");
    }

    // ── Cached Repository ─────────────────────────────────────────────────────

    [Fact]
    public void CachedRepository_ReturnsNull_WhenNotCached()
    {
        var repo = new InMemoryProductRepo();
        repo.GetById(999).Should().BeNull();
    }

    [Fact]
    public void CachedRepository_ReturnsCachedItem()
    {
        var repo = new InMemoryProductRepo();
        repo.Cache(42, "Thingamajig");

        repo.GetById(42).Should().Be("Thingamajig");
    }
}
