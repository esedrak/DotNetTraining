using Moq;

namespace DotNetTraining.Basics.Mocking;

// ── Interfaces to mock ────────────────────────────────────────────────────────

public record Product(int Id, string Name, decimal Price);

public interface IProductRepository
{
    Task<Product?> GetByIdAsync(int id, CancellationToken ct = default);
    Task<IReadOnlyList<Product>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(Product product, CancellationToken ct = default);
}

public interface IEmailService
{
    Task SendAsync(string to, string subject, string body, CancellationToken ct = default);
}

// ── Service under test ────────────────────────────────────────────────────────

public class ProductService(IProductRepository repo, IEmailService email)
{
    public async Task<Product> GetProductAsync(int id, CancellationToken ct = default)
    {
        var product = await repo.GetByIdAsync(id, ct);
        return product ?? throw new KeyNotFoundException($"Product {id} not found.");
    }

    public async Task NotifyLowStockAsync(string adminEmail, CancellationToken ct = default)
    {
        var products = await repo.GetAllAsync(ct);
        if (products.Count == 0)
            await email.SendAsync(adminEmail, "Low Stock Alert", "No products in catalog.", ct);
    }
}

// ── Mock setup examples (used in tests) ──────────────────────────────────────

/// <summary>
/// Helper factory methods that demonstrate Moq patterns.
/// In real tests, create mocks directly in each test method.
/// </summary>
public static class MockExamples
{
    /// <summary>
    /// Demonstrates: Setup → Returns → Verify
    /// Equivalent to: mock.On("GetById").Return(product) + mock.AssertCalled(...)
    /// </summary>
    public static Mock<IProductRepository> CreateRepoWithProduct(Product product)
    {
        var mock = new Mock<IProductRepository>();

        mock.Setup(r => r.GetByIdAsync(product.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(product);

        return mock;
    }

    /// <summary>
    /// Demonstrates: Setup → ThrowsAsync
    /// Equivalent to: mock.On("GetById").Return(nil, errors.New("not found"))
    /// </summary>
    public static Mock<IProductRepository> CreateRepoThatThrows(int id, Exception ex)
    {
        var mock = new Mock<IProductRepository>();

        mock.Setup(r => r.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ThrowsAsync(ex);

        return mock;
    }

    /// <summary>
    /// Demonstrates: Callback — capture arguments passed to the mock.
    /// </summary>
    public static (Mock<IProductRepository> Mock, List<Product> Captured) CreateCapturingRepo()
    {
        var captured = new List<Product>();
        var mock = new Mock<IProductRepository>();

        mock.Setup(r => r.AddAsync(It.IsAny<Product>(), It.IsAny<CancellationToken>()))
            .Callback<Product, CancellationToken>((p, _) => captured.Add(p))
            .Returns(Task.CompletedTask);

        return (mock, captured);
    }
}
