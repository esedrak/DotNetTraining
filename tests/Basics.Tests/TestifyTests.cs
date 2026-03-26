using DotNetTraining.Basics.Testify;
using FluentAssertions;

namespace Basics.Tests;

/// <summary>
/// Demonstrates FluentAssertions — the C# equivalent of Go's testify library.
///
/// | testify                             | FluentAssertions                          |
/// |-------------------------------------|-------------------------------------------|
/// | assert.Equal(t, expected, actual)   | actual.Should().Be(expected)              |
/// | assert.True(t, cond)                | cond.Should().BeTrue()                    |
/// | assert.Contains(t, list, elem)      | list.Should().Contain(elem)               |
/// | assert.Error(t, err)                | act.Should().Throw&lt;Exception&gt;()         |
/// | assert.NoError(t, err)              | act.Should().NotThrow()                   |
/// | assert.ErrorIs(t, err, target)      | act.Should().Throw&lt;T&gt;()                 |
/// | require.NoError(t, err) — halts     | no direct equivalent; use act.Should().NotThrow() |
/// </summary>
public class TestifyTests
{
    private readonly ProductCatalog _catalog = new();

    // ── Basic value assertions ────────────────────────────────────────────────

    [Fact]
    public void FindById_ReturnsProduct_WhenPresent()
    {
        var product = _catalog.FindById(1);

        product.Should().NotBeNull();
        product!.Name.Should().Be("Widget");
        product.Price.Should().Be(9.99m);
    }

    [Fact]
    public void FindById_ReturnsNull_WhenAbsent()
    {
        _catalog.FindById(999).Should().BeNull();
    }

    // ── Collection assertions ─────────────────────────────────────────────────

    [Fact]
    public void GetAll_ReturnsAllProducts()
    {
        var products = _catalog.GetAll();

        products.Should().HaveCount(2);
        products.Should().Contain(p => p.Name == "Widget");
        products.Should().AllSatisfy(p => p.Price.Should().BeGreaterThan(0));
    }

    // ── Exception assertions ──────────────────────────────────────────────────

    [Fact]
    public void GetById_Throws_WhenNotFound()
    {
        var act = () => _catalog.GetById(999);

        // Equivalent to assert.ErrorAs(t, err, &target) — checks type and message
        act.Should().Throw<ProductNotFoundException>()
           .WithMessage("*999*");
    }

    // ── Numeric / comparison assertions ──────────────────────────────────────

    [Fact]
    public void Product_Price_IsBetweenBounds()
    {
        var widget = _catalog.GetById(1);

        widget.Price.Should().BeGreaterThan(0).And.BeLessThanOrEqualTo(100m);
        widget.Price.Should().BeApproximately(9.99m, 0.001m);
    }

    // ── String assertions ─────────────────────────────────────────────────────

    [Fact]
    public void Product_Name_IsNonEmptyAndStartsWithUppercase()
    {
        var widget = _catalog.GetById(1);

        widget.Name.Should().NotBeNullOrWhiteSpace();
        widget.Name.Should().MatchRegex("^[A-Z]");
    }

    // ── Boolean assertions ────────────────────────────────────────────────────

    [Fact]
    public void Add_ReturnsTrue_ForNewProduct()
    {
        var added = _catalog.Add(new Product(99, "Thingamajig", 4.99m));
        added.Should().BeTrue();
    }

    [Fact]
    public void Add_ReturnsFalse_ForDuplicateId()
    {
        var duplicate = _catalog.Add(new Product(1, "Duplicate", 0m));
        duplicate.Should().BeFalse();
    }
}
