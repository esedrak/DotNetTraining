using DotNetTraining.Basics.ConditionalCompilation;
using FluentAssertions;

namespace Basics.Tests;

/// <summary>
/// Tests for C# conditional compilation: preprocessor directives and runtime platform detection.
/// </summary>
public class ConditionalCompilationTests
{
    // ── Runtime OS detection ──────────────────────────────────────────────────

    [Fact]
    public void RuntimeOsPlatform_ReturnsKnownValue()
    {
        var os = ConditionalCompilation.RuntimeOsPlatform();
        os.Should().BeOneOf("windows", "linux", "macos");
    }

    // ── Runtime architecture ──────────────────────────────────────────────────

    [Fact]
    public void RuntimeArchitecture_ReturnsKnownValue()
    {
        var arch = ConditionalCompilation.RuntimeArchitecture();
        arch.Should().BeOneOf("x64", "arm64", "x86", "unknown");
    }

    // ── Build mode ────────────────────────────────────────────────────────────

    [Fact]
    public void BuildMode_ReturnsExpectedConfiguration()
    {
        var mode = ConditionalCompilation.BuildMode();
        // Tests run in Debug configuration by default
        mode.Should().BeOneOf("debug", "release");
    }

    // ── Platform greeting uses compile-time constant ──────────────────────────

    [Fact]
    public void PlatformGreeting_ReturnsNonEmptyString()
    {
        // The exact value depends on which #if symbol was defined at compile time.
        // This test simply verifies the method compiles and returns something.
        var greeting = ConditionalCompilation.PlatformGreeting();
        greeting.Should().NotBeNullOrWhiteSpace();
    }
}
