namespace DotNetTraining.Basics.FluentAssertions;

// ── Types used by FluentAssertions examples ───────────────────────────────────

public record Product(int Id, string Name, decimal Price);

/// <summary>
/// Domain exception for "not found" cases — used to demonstrate assertion on
/// exception type and message.
/// </summary>
public class ProductNotFoundException(int id)
    : Exception($"Product {id} was not found.")
{ }

/// <summary>
/// Simple service whose methods are exercised in the FluentAssertions test
/// examples (see tests/Basics.Tests/TestifyTests.cs).
/// </summary>
public class ProductCatalog
{
    private readonly Dictionary<int, Product> _products = new()
    {
        [1] = new Product(1, "Widget", 9.99m),
        [2] = new Product(2, "Gadget", 24.99m),
    };

    public Product? FindById(int id)
        => _products.GetValueOrDefault(id);

    public Product GetById(int id)
        => _products.TryGetValue(id, out var p)
            ? p
            : throw new ProductNotFoundException(id);

    public IReadOnlyList<Product> GetAll()
        => _products.Values.ToList();

    public bool Add(Product product)
    {
        if (_products.ContainsKey(product.Id))
        {
            return false;
        }

        _products[product.Id] = product;
        return true;
    }
}
