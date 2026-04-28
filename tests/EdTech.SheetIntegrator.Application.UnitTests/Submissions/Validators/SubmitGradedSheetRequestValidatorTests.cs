using EdTech.SheetIntegrator.Application.Submissions.Dtos;
using EdTech.SheetIntegrator.Application.Submissions.Validators;

namespace EdTech.SheetIntegrator.Application.UnitTests.Submissions.Validators;

public class SubmitGradedSheetRequestValidatorTests
{
    private readonly SubmitGradedSheetRequestValidator _validator = new();

    [Fact]
    public void Valid_Request_Passes()
    {
        var req = new SubmitGradedSheetRequest(
            Guid.NewGuid(), "alice", "alice.xlsx", "x", new MemoryStream());

        _validator.Validate(req).IsValid.Should().BeTrue();
    }

    [Fact]
    public void Empty_AssessmentId_Fails()
    {
        var req = new SubmitGradedSheetRequest(
            Guid.Empty, "alice", "alice.xlsx", "x", new MemoryStream());

        _validator.Validate(req).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_StudentIdentifier_Fails(string id)
    {
        var req = new SubmitGradedSheetRequest(
            Guid.NewGuid(), id, "alice.xlsx", "x", new MemoryStream());

        _validator.Validate(req).IsValid.Should().BeFalse();
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void Empty_SourceFileName_Fails(string name)
    {
        var req = new SubmitGradedSheetRequest(
            Guid.NewGuid(), "alice", name, "x", new MemoryStream());

        _validator.Validate(req).IsValid.Should().BeFalse();
    }

    [Fact]
    public void Null_FileStream_Fails()
    {
        var req = new SubmitGradedSheetRequest(
            Guid.NewGuid(), "alice", "alice.xlsx", "x", null!);

        _validator.Validate(req).IsValid.Should().BeFalse();
    }
}
