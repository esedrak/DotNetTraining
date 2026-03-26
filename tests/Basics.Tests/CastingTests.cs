using DotNetTraining.Basics.Casting;
using FluentAssertions;

namespace Basics.Tests;

public class CastingTests
{
    [Fact]
    public void WhatIsIt_ReturnsCorrectTypeDescription()
    {
        TypeCheckingExamples.WhatIsIt("hello").Should().Be("string: hello");
        TypeCheckingExamples.WhatIsIt(42).Should().Be("int: 42");
        TypeCheckingExamples.WhatIsIt(true).Should().Be("bool: True");
        TypeCheckingExamples.WhatIsIt(null!).Should().Be("null");
    }

    [Fact]
    public void WhatIsIt_RecognizesDog()
    {
        var dog = new Dog { Breed = "Labrador" };
        TypeCheckingExamples.WhatIsIt(dog).Should().Contain("Labrador");
    }

    [Fact]
    public void Describe_UsesPatternMatching()
    {
        IAnimal dog = new Dog { Breed = "Poodle" };
        IAnimal cat = new Cat();

        TypeCheckingExamples.Describe(dog).Should().Contain("Poodle");
        TypeCheckingExamples.Describe(cat).Should().Be("Cat");
    }

    [Fact]
    public void GetBreed_ReturnsBreed_ForDog()
    {
        IAnimal dog = new Dog { Breed = "Beagle" };
        TypeCheckingExamples.GetBreed(dog).Should().Be("Beagle");
    }

    [Fact]
    public void GetBreed_ReturnsNull_ForNonDog()
    {
        IAnimal cat = new Cat();
        TypeCheckingExamples.GetBreed(cat).Should().BeNull();
    }

    [Fact]
    public void SafeNarrow_ThrowsOverflowException_ForLargeValue()
    {
        var act = () => ConversionExamples.SafeNarrow(256L);
        act.Should().Throw<OverflowException>();
    }

    [Fact]
    public void SafeNarrow_Succeeds_WithinRange()
    {
        ConversionExamples.SafeNarrow(255L).Should().Be(255);
    }
}
