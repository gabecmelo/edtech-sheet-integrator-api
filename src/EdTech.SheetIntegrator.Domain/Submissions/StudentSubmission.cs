using EdTech.SheetIntegrator.Domain.Common;
using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;

namespace EdTech.SheetIntegrator.Domain.Submissions;

/// <summary>
/// Aggregate root: a student's uploaded sheet, optionally with the grading result attached.
/// Lifecycle: created with answers (ungraded) -&gt; <see cref="AttachResult"/> sets the result
/// once and only once, raising <see cref="SubmissionGraded"/>.
/// </summary>
public sealed class StudentSubmission : AggregateRoot<Guid>
{
    private readonly List<Answer> _answers = [];

    public Guid AssessmentId { get; private set; }

    public string StudentIdentifier { get; private set; } = null!;

    public string SourceFileName { get; private set; } = null!;

    public DateTimeOffset SubmittedAt { get; private set; }

    public IReadOnlyList<Answer> Answers => _answers.AsReadOnly();

    public GradingResult? Result { get; private set; }

    public bool IsGraded => Result is not null;

    private StudentSubmission()
    {
    }

    public StudentSubmission(
        Guid id,
        Guid assessmentId,
        string studentIdentifier,
        IEnumerable<Answer> answers,
        string sourceFileName,
        DateTimeOffset submittedAt)
        : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Submission id is required.");
        }

        if (assessmentId == Guid.Empty)
        {
            throw new DomainException("Assessment id is required.");
        }

        if (string.IsNullOrWhiteSpace(studentIdentifier))
        {
            throw new DomainException("Student identifier is required.");
        }

        if (string.IsNullOrWhiteSpace(sourceFileName))
        {
            throw new DomainException("Source file name is required.");
        }

        var answerList = answers?.ToList() ?? throw new DomainException("Answers cannot be null.");
        if (answerList.Count == 0)
        {
            throw new DomainException("Submission must contain at least one answer.");
        }

        AssessmentId = assessmentId;
        StudentIdentifier = studentIdentifier;
        SourceFileName = sourceFileName;
        SubmittedAt = submittedAt;
        _answers.AddRange(answerList);
    }

    /// <summary>Attach the grading result. Idempotent guard: throws if already graded.</summary>
    public void AttachResult(GradingResult result)
    {
        if (Result is not null)
        {
            throw new DomainException("Submission has already been graded.");
        }

        Result = result;
        Raise(new SubmissionGraded(Id, AssessmentId, result.Score, result.GradedAt));
    }
}
