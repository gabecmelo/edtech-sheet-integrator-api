using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Application.Assessments.Dtos;

/// <summary>Inbound DTO describing one question in a CreateAssessment request.</summary>
public sealed record QuestionInput(
    string QuestionId,
    string Prompt,
    string CorrectAnswer,
    decimal Points,
    MatchMode MatchMode,
    decimal? NumericTolerance);
