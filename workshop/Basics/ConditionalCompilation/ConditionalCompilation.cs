using System.Runtime.InteropServices;

namespace DotNetTraining.Basics.ConditionalCompilation;

// ── #if directives — conditional compilation ─────────────────────────────────
//
// Define symbols via <DefineConstants> in .csproj, or -p:DefineConstants=LINUX
// C# preprocessor directives are inline within a file.

public static class ConditionalCompilation
{
    /// <summary>
    /// Returns a platform greeting compiled only for specific targets.
    /// </summary>
    public static string PlatformGreeting()
    {
#if WINDOWS
        return "Hello from Windows!";
#elif LINUX
        return "Hello from Linux!";
#elif MACOS
        return "Hello from macOS!";
#else
        return "Hello from an unknown platform!";
#endif
    }

    /// <summary>
    /// DEBUG vs RELEASE — the most common conditional compilation.
    /// Defined automatically by the build configuration.
    /// </summary>
    public static string BuildMode()
    {
#if DEBUG
        return "debug";
#else
        return "release";
#endif
    }

    /// <summary>
    /// Runtime OS check — no recompilation needed.
    /// </summary>
    public static string RuntimeOsPlatform()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            return "windows";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
        {
            return "linux";
        }

        if (RuntimeInformation.IsOSPlatform(OSPlatform.OSX))
        {
            return "darwin";
        }

        return "unknown";
    }

    /// <summary>
    /// Runtime architecture detection.
    /// </summary>
    public static string RuntimeArchitecture()
        => RuntimeInformation.ProcessArchitecture switch
        {
            Architecture.X64 => "amd64",
            Architecture.Arm64 => "arm64",
            Architecture.X86 => "386",
            _ => "unknown"
        };
}

// ── Compile-time constants ────────────────────────────────────────────────────

/// <summary>
/// Constants evaluated at compile time (not build-tagged, but similar concept).
/// </summary>
public static class BuildConstants
{
    // Define custom symbols in .csproj:
    // <DefineConstants>$(DefineConstants);MY_FEATURE</DefineConstants>

#if MY_FEATURE
    public const bool MyFeatureEnabled = true;
#else
    public const bool MyFeatureEnabled = false;
#endif
}
