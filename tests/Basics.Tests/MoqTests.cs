using DotNetTraining.Basics.Mocking;
using FluentAssertions;
using Moq;

namespace Basics.Tests;

/// <summary>
/// Demonstrates Moq
/// Pattern: Setup → Act → Verify  (analogous to On → Execute → AssertCalled)
/// </summary>
public class MoqTests
{
    // ── Setup → Returns ───────────────────────────────────────────────────────

    [Fact]
    public async Task GetProduct_ReturnsProduct_WhenRepoHasIt()
    {
        var product = new Product(1, "Widget", 9.99m);
        var repoMock = MockExamples.CreateRepoWithProduct(product);
        var emailMock = new Mock<IEmailService>();

        var svc = new ProductService(repoMock.Object, emailMock.Object);

        var result = await svc.GetProductAsync(1);

        result.Should().BeEquivalentTo(product);
        repoMock.Verify(r => r.GetByIdAsync(1, It.IsAny<CancellationToken>()), Times.Once);
    }

    // ── Returns null → service throws ────────────────────────────────────────

    [Fact]
    public async Task GetProduct_Throws_WhenNotFound()
    {
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetByIdAsync(99, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Product?)null);

        var svc = new ProductService(repoMock.Object, new Mock<IEmailService>().Object);

        var act = () => svc.GetProductAsync(99);
        await act.Should().ThrowAsync<KeyNotFoundException>();
    }

    // ── ThrowsAsync ───────────────────────────────────────────────────────────

    [Fact]
    public async Task GetProduct_Rethrows_WhenRepoThrows()
    {
        var ex = new InvalidOperationException("DB down");
        var repoMock = MockExamples.CreateRepoThatThrows(1, ex);

        var svc = new ProductService(repoMock.Object, new Mock<IEmailService>().Object);

        var act = () => svc.GetProductAsync(1);
        await act.Should().ThrowAsync<InvalidOperationException>().WithMessage("DB down");
    }

    // ── Verify — email was sent ───────────────────────────────────────────────

    [Fact]
    public async Task NotifyLowStock_SendsEmail_WhenCatalogEmpty()
    {
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([]);

        var emailMock = new Mock<IEmailService>();

        var svc = new ProductService(repoMock.Object, emailMock.Object);
        await svc.NotifyLowStockAsync("admin@example.com");

        emailMock.Verify(e => e.SendAsync(
            "admin@example.com",
            "Low Stock Alert",
            It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task NotifyLowStock_DoesNotSendEmail_WhenProductsExist()
    {
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
                .ReturnsAsync([new Product(1, "Widget", 1m)]);

        var emailMock = new Mock<IEmailService>();

        var svc = new ProductService(repoMock.Object, emailMock.Object);
        await svc.NotifyLowStockAsync("admin@example.com");

        emailMock.Verify(e => e.SendAsync(
            It.IsAny<string>(), It.IsAny<string>(), It.IsAny<string>(),
            It.IsAny<CancellationToken>()), Times.Never);
    }

    // ── Callback — capture arguments ──────────────────────────────────────────

    [Fact]
    public async Task CapturingRepo_RecordsAddedProducts()
    {
        var (repoMock, captured) = MockExamples.CreateCapturingRepo();

        await repoMock.Object.AddAsync(new Product(1, "A", 1m));
        await repoMock.Object.AddAsync(new Product(2, "B", 2m));

        captured.Should().HaveCount(2);
        captured[0].Name.Should().Be("A");
        captured[1].Name.Should().Be("B");
    }
}
