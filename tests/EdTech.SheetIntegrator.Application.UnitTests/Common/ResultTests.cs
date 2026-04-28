using EdTech.SheetIntegrator.Application.Common;

namespace EdTech.SheetIntegrator.Application.UnitTests.Common;

public class ResultTests
{
    [Fact]
    public void Success_Sets_IsSuccess_And_Exposes_Value()
    {
        var result = Result<int>.Success(42);

        result.IsSuccess.Should().BeTrue();
        result.IsFailure.Should().BeFalse();
        result.Error.Should().BeNull();
        result.Value.Should().Be(42);
    }

    [Fact]
    public void Failure_Sets_IsFailure_And_Exposes_Error()
    {
        var error = new Error("boom", "kaboom");
        var result = Result<int>.Failure(error);

        result.IsSuccess.Should().BeFalse();
        result.IsFailure.Should().BeTrue();
        result.Error.Should().Be(error);
    }

    [Fact]
    public void Accessing_Value_On_Failed_Result_Throws()
    {
        var result = Result<int>.Failure(new Error("boom", "k"));

        var act = () => _ = result.Value;

        act.Should().Throw<InvalidOperationException>();
    }

    [Fact]
    public void Implicit_Conversion_From_Value_Builds_Success()
    {
        Result<string> result = "hello";

        result.IsSuccess.Should().BeTrue();
        result.Value.Should().Be("hello");
    }

    [Fact]
    public void Implicit_Conversion_From_Error_Builds_Failure()
    {
        Result<string> result = new Error("e", "msg");

        result.IsFailure.Should().BeTrue();
        result.Error!.Code.Should().Be("e");
    }
}
