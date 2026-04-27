namespace EdTech.SheetIntegrator.Domain.Exceptions;

public sealed class InvalidQuestionConfigurationException : DomainException
{
    public InvalidQuestionConfigurationException(string message)
        : base(message)
    {
    }
}
