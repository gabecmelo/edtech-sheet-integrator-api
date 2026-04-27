using EdTech.SheetIntegrator.Domain.Common;
using EdTech.SheetIntegrator.Domain.Exceptions;
using EdTech.SheetIntegrator.Domain.Grading;
using EdTech.SheetIntegrator.Domain.Submissions;

namespace EdTech.SheetIntegrator.Domain.Assessments;

/// <summary>
/// Aggregate root: an assessment definition (title + answer key). Owns the grading logic
/// because the rules (correct answers, points, match modes) live on its <see cref="Question"/>s.
/// </summary>
public sealed class Assessment : AggregateRoot<Guid>
{
    private readonly List<Question> _questions = [];

    public string Title { get; private set; } = null!;

    public DateTimeOffset CreatedAt { get; private set; }

    public IReadOnlyList<Question> Questions => _questions.AsReadOnly();

    public decimal MaxScore => _questions.Sum(q => q.Points);

    private Assessment()
    {
    }

    public Assessment(Guid id, string title, IEnumerable<Question> questions, DateTimeOffset createdAt)
        : base(id)
    {
        if (id == Guid.Empty)
        {
            throw new DomainException("Assessment id is required.");
        }

        if (string.IsNullOrWhiteSpace(title))
        {
            throw new DomainException("Assessment title is required.");
        }

        var list = questions?.ToList() ?? throw new DomainException("Questions cannot be null.");
        if (list.Count == 0)
        {
            throw new DomainException("Assessment must contain at least one question.");
        }

        var duplicates = list
            .GroupBy(q => q.QuestionId, StringComparer.Ordinal)
            .Where(g => g.Count() > 1)
            .Select(g => g.Key)
            .ToArray();

        if (duplicates.Length > 0)
        {
            throw new InvalidQuestionConfigurationException(
                $"Duplicate question ids in assessment: {string.Join(", ", duplicates)}.");
        }

        Title = title;
        CreatedAt = createdAt;
        _questions.AddRange(list);
    }

    /// <summary>
    /// Pure grading function. Produces a <see cref="GradingResult"/> for the supplied answers
    /// without any I/O. Unanswered questions count as incorrect (zero points). Extra answers
    /// referencing unknown questions cause a <see cref="MismatchedAnswerSheetException"/>.
    /// </summary>
    public GradingResult Grade(IReadOnlyList<Answer> answers, DateTimeOffset gradedAt)
    {
        if (answers is null)
        {
            throw new MismatchedAnswerSheetException("Answers collection cannot be null.");
        }

        var responsesByQuestion = new Dictionary<string, string>(StringComparer.Ordinal);
        foreach (var answer in answers)
        {
            // Last response for a given question id wins (sheets occasionally repeat columns).
            responsesByQuestion[answer.QuestionId] = answer.Response;
        }

        var knownIds = new HashSet<string>(_questions.Select(q => q.QuestionId), StringComparer.Ordinal);
        var unknown = responsesByQuestion.Keys.Where(id => !knownIds.Contains(id)).ToArray();
        if (unknown.Length > 0)
        {
            throw new MismatchedAnswerSheetException(
                $"Submission references unknown question ids: {string.Join(", ", unknown)}.");
        }

        var outcomes = new List<QuestionOutcome>(_questions.Count);
        foreach (var question in _questions)
        {
            var hasResponse = responsesByQuestion.TryGetValue(question.QuestionId, out var response);
            var isCorrect = hasResponse && question.Matches(response);
            var earned = isCorrect ? question.Points : 0m;
            outcomes.Add(new QuestionOutcome(question.QuestionId, isCorrect, earned, question.Points));
        }

        var earnedTotal = outcomes.Sum(o => o.EarnedPoints);
        var score = new Score(earnedTotal, MaxScore);

        return new GradingResult(score, outcomes, gradedAt);
    }
}
