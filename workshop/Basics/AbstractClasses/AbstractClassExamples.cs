namespace DotNetTraining.Basics.AbstractClasses;

// ── Abstract base with abstract and virtual members ───────────────────────────

/// <summary>
/// Abstract class — cannot be instantiated directly.
/// Combines abstract members (must override) with virtual members (can override)
/// and concrete members (inherited as-is).
/// </summary>
public abstract class Shape
{
    /// <summary>Abstract: no implementation. Subclasses MUST provide one.</summary>
    public abstract double Area();

    /// <summary>Abstract: no implementation. Subclasses MUST provide one.</summary>
    public abstract double Perimeter();

    /// <summary>
    /// Virtual: has a default implementation. Subclasses CAN override it.
    /// Calls the abstract members — polymorphism in action.
    /// </summary>
    public virtual string Describe() =>
        $"{GetType().Name}: area={Area():F2}, perimeter={Perimeter():F2}";

    /// <summary>
    /// Non-virtual concrete method: all subclasses inherit this exactly.
    /// Cannot be overridden.
    /// </summary>
    public bool IsLargerThan(Shape other) => Area() > other.Area();
}

public class Circle(double radius) : Shape
{
    public double Radius => radius;

    public override double Area() => Math.PI * radius * radius;
    public override double Perimeter() => 2 * Math.PI * radius;
    // Does NOT override Describe() — uses the base implementation
}

public class Rectangle(double width, double height) : Shape
{
    public override double Area() => width * height;
    public override double Perimeter() => 2 * (width + height);

    // Overrides the virtual method with a more specific description
    public override string Describe() =>
        $"Rectangle {width}×{height}: area={Area():F2}";
}

/// <summary>
/// `sealed` on a class prevents further subclassing.
/// Use on leaf nodes to signal the design is complete and enable JIT optimisations.
/// </summary>
public sealed class Square(double side) : Rectangle(side, side);

// ── Template Method Pattern ───────────────────────────────────────────────────

/// <summary>
/// Template Method: the abstract base defines the algorithm skeleton;
/// subclasses supply the individual steps.
/// </summary>
public abstract class DataExporter
{
    /// <summary>
    /// Template method — the overall algorithm is fixed here.
    /// The steps (Transform, Format) are delegated to subclasses.
    /// </summary>
    public async Task<string> ExportAsync(IEnumerable<string> data)
    {
        var transformed = Transform(data);
        var formatted = Format(transformed);
        return await WriteAsync(formatted);
    }

    protected abstract IEnumerable<string> Transform(IEnumerable<string> data);
    protected abstract string Format(IEnumerable<string> data);

    /// <summary>
    /// Virtual with a sensible default — subclasses can override to write to a file,
    /// stream, etc. By default just returns the content string.
    /// </summary>
    protected virtual Task<string> WriteAsync(string content) =>
        Task.FromResult(content);
}

public class UpperCaseCsvExporter : DataExporter
{
    protected override IEnumerable<string> Transform(IEnumerable<string> data) =>
        data.Select(s => s.ToUpperInvariant());

    protected override string Format(IEnumerable<string> data) =>
        string.Join(",", data);
}

// ── Abstract class with shared state ─────────────────────────────────────────

/// <summary>
/// Abstract class with concrete state and behaviour — shows when to prefer abstract
/// class over interface: when subclasses genuinely share implementation, not just contracts.
/// </summary>
public abstract class CachedRepository<T>
{
    private readonly Dictionary<int, T> _cache = new();

    /// <summary>Returns a cached item if present; default (null for reference types) otherwise.</summary>
    public T? GetById(int id) =>
        _cache.TryGetValue(id, out var item) ? item : default;

    /// <summary>Stores an item in the cache.</summary>
    public void Cache(int id, T item) => _cache[id] = item;

    /// <summary>
    /// Abstract: subclasses define where to fetch uncached items.
    /// The base class does not know about databases, files, or APIs.
    /// </summary>
    protected abstract T FetchFromStore(int id);
}

public class InMemoryProductRepo : CachedRepository<string>
{
    private readonly Dictionary<int, string> _store =
        new() { [1] = "Widget", [2] = "Gadget" };

    protected override string FetchFromStore(int id) =>
        _store.TryGetValue(id, out var v)
            ? v
            : throw new KeyNotFoundException($"Product {id} not found");
}
