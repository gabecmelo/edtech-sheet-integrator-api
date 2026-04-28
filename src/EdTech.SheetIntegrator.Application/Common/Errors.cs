namespace EdTech.SheetIntegrator.Application.Common;

/// <summary>Catalog of well-known errors emitted by use cases. Codes are part of the API contract.</summary>
public static class Errors
{
    public static class Validation
    {
        public static Error Failed(IEnumerable<string> messages) =>
            new("validation.failed", string.Join("; ", messages));
    }

    public static class Assessment
    {
        public static readonly Error NotFound = new("assessment.not_found", "Assessment was not found.");

        public static Error InvalidConfiguration(string detail) =>
            new("assessment.invalid_configuration", detail);
    }

    public static class Submission
    {
        public static readonly Error NotFound = new("submission.not_found", "Submission was not found.");

        public static Error UnsupportedFileType(string detail) =>
            new("submission.unsupported_file_type", detail);

        public static Error UnreadableSheet(string detail) =>
            new("submission.unreadable_sheet", detail);

        public static Error MismatchedAnswerSheet(string detail) =>
            new("submission.mismatched_answer_sheet", detail);

        public static Error Invalid(string detail) =>
            new("submission.invalid", detail);
    }
}
