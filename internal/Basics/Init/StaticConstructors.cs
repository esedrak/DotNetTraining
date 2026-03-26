using System.Runtime.CompilerServices;

namespace DotNetTraining.Basics.Init;

// ── Static constructor — equivalent to Go's func init() ──────────────────────

/// <summary>
/// Static constructors run automatically, once, before the first use of the class.
/// Equivalent to Go's `func init()` in a package.
/// You cannot call a static constructor manually.
/// </summary>
public class AppConfig
{
    // Static field initializers run before the static constructor
    public static readonly string AppName = "DotNetTraining";

    public static readonly string ConnectionString;
    public static readonly int MaxRetries;
    public static readonly bool IsDebug;

    // Static constructor — runs after static field initializers
    static AppConfig()
    {
        ConnectionString = Environment.GetEnvironmentVariable("DB_URL")
            ?? "Host=localhost;Port=5432;Database=dotnetbank;Username=dotnettrainer;Password=verysecret";

        MaxRetries = int.TryParse(
            Environment.GetEnvironmentVariable("MAX_RETRIES"), out int retries) ? retries : 3;

        IsDebug = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") == "Development";
    }
}

// ── Execution order demonstration ────────────────────────────────────────────

public class InitializationOrder
{
    // 1. Static field initializer (first)
    public static readonly List<string> Log = ["static field initialized"];

    // 2. Static constructor (second)
    static InitializationOrder()
    {
        Log.Add("static constructor called");
    }

    // 3. Instance constructor (third, on each new())
    public InitializationOrder()
    {
        Log.Add("instance constructor called");
    }
}

// ── [ModuleInitializer] — assembly-level init ─────────────────────────────────

/// <summary>
/// [ModuleInitializer] runs once when the assembly loads, before any user code.
/// More like Go's package-level init() than a class static constructor.
/// Must be: internal, static, void, no parameters.
/// </summary>
internal static class ModuleStartup
{
    private static readonly List<string> _initLog = [];

    public static IReadOnlyList<string> InitLog => _initLog;

    // CA2255: [ModuleInitializer] is intended for application entry points, not libraries.
    // Suppressed here because this is an educational example demonstrating the attribute.
#pragma warning disable CA2255
    [ModuleInitializer]
    internal static void Initialize()
#pragma warning restore CA2255
    {
        _initLog.Add($"Module initialized at {DateTime.UtcNow:O}");
    }
}

// ── Lazy<T> — deferred initialization (preferred over static constructors for heavy work) ──

/// <summary>
/// Lazy&lt;T&gt; is thread-safe and defers initialization until first access.
/// Use this for expensive resources (DB connections, caches) instead of static constructors.
/// Equivalent to sync.Once in Go.
/// </summary>
public static class ExpensiveResource
{
    private static readonly Lazy<string> _connectionString = new(() =>
    {
        // Expensive initialization — only runs on first access
        System.Threading.Thread.Sleep(1); // simulate work
        return "Host=localhost;Database=dotnetbank";
    }, isThreadSafe: true);

    public static string ConnectionString => _connectionString.Value;
}
