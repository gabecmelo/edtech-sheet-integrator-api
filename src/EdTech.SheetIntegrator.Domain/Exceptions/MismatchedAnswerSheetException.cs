namespace EdTech.SheetIntegrator.Domain.Exceptions;

public sealed class MismatchedAnswerSheetException : DomainException
{
    public MismatchedAnswerSheetException(string message)
        : base(message)
    {
    }
}
