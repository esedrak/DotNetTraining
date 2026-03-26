using DotNetTraining.Basics.ErrorHandling;
using FluentAssertions;

namespace Basics.Tests;

public class ErrorHandlingTests
{
    [Fact]
    public void GetAccount_ReturnsName_WhenFound()
    {
        var service = new BankService();
        service.GetAccount(1).Should().Be("Alice");
    }

    [Fact]
    public void GetAccount_ThrowsNotFoundException_WhenMissing()
    {
        var service = new BankService();
        var act = () => service.GetAccount(99);

        act.Should().Throw<NotFoundException>()
            .Where(ex => ex.Id.Equals(99))
            .WithMessage("*99*");
    }

    [Fact]
    public void GetAccountWithFilter_ReturnsPlaceholder_WhenNotFound()
    {
        var service = new BankService();
        var result = service.GetAccountWithFilter(99);
        result.Should().Contain("Not Found");
    }

    [Fact]
    public void ParsePositive_ReturnsOk_ForValidInput()
    {
        var result = ResultExamples.ParsePositive("42");
        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void ParsePositive_ReturnsFail_ForNonNumeric()
    {
        var result = ResultExamples.ParsePositive("abc");
        result.IsSuccess.Should().BeFalse();
        result.Error.Should().Contain("not a valid integer");
    }

    [Fact]
    public void ParsePositive_ReturnsFail_ForNegativeNumber()
    {
        var result = ResultExamples.ParsePositive("-5");
        result.IsSuccess.Should().BeFalse();
    }

    [Fact]
    public void Result_Map_TransformsValue_WhenSuccessful()
    {
        var result = ResultExamples.ParsePositive("10")
            .Map(x => x * 2);

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be(20);
    }

    [Fact]
    public void Result_Map_PropagatesFailure()
    {
        var result = ResultExamples.ParsePositive("abc")
            .Map(x => x * 2);

        result.IsSuccess.Should().BeFalse();
    }
}
