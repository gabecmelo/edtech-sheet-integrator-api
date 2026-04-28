namespace EdTech.SheetIntegrator.Application.Assessments.Dtos;

public sealed record CreateAssessmentRequest(string Title, IReadOnlyList<QuestionInput> Questions);
