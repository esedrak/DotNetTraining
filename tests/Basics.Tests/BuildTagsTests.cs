using DotNetTraining.Basics.BuildTags;
using FluentAssertions;

namespace Basics.Tests;

/// <summary>
/// Demonstrates C# conditional compilation — equivalent of Go's build tags.
///
/// | Go                          | C#                                          |
/// |-----------------------------|---------------------------------------------|
/// | //go:build linux            | #if LINUX (set via DefineConstants in .csproj) |
/// | //go:build !debug           | #if !DEBUG                                  |
/// | runtime.GOOS                | RuntimeInformation.IsOSPlatform()           |
/// | runtime.GOARCH              | RuntimeInformation.ProcessArchitecture      |
/// </summary>
public class BuildTagsTests
{
    // ── Runtime OS detection ──────────────────────────────────────────────────

    [Fact]
    public void RuntimeOsPlatform_ReturnsKnownValue()
    {
        var os = ConditionalCompilation.RuntimeOsPlatform();
        os.Should().BeOneOf("windows", "linux", "darwin");
    }

    // ── Runtime architecture ──────────────────────────────────────────────────

    [Fact]
    public void RuntimeArchitecture_ReturnsKnownValue()
    {
        var arch = ConditionalCompilation.RuntimeArchitecture();
        arch.Should().BeOneOf("amd64", "arm64", "386", "unknown");
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
