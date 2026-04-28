using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using EdTech.SheetIntegrator.Application.Submissions.Validators;

namespace EdTech.SheetIntegrator.Application.UnitTests.Submissions.Validators;

public class ListSubmissionsRequestValidatorTests
{
    private readonly ListSubmissionsRequestValidator _validator = new();

    [Fact]
    public void Default_Page_And_Size_Pass()
    {
        var req = new ListSubmissionsRequest(Guid.NewGuid());

        _validator.Validate(req).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Page_Below_One_Fails()
    {
        _validator.Validate(new ListSubmissionsRequest(Guid.NewGuid(), Page: 0)).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData(0)]
    [InlineData(201)]
    public void PageSize_Outside_Range_Fails(int size)
    {
        _validator.Validate(new ListSubmissionsRequest(Guid.NewGuid(), PageSize: size)).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Empty_AssessmentId_Fails()
    {
        _validator.Validate(new ListSubmissionsRequest(Guid.Empty)).IsValid.Should().BeFalse();
    }
}
