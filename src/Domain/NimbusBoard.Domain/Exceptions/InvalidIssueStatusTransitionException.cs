using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Domain.Exceptions;

public sealed class InvalidIssueStatusTransitionException : Exception
{
    public IssueStatus From { get; }
    public IssueStatus To { get; }

    public InvalidIssueStatusTransitionException(IssueStatus from, IssueStatus to)
        : base($"Cannot move issue from {from} to {to}.")
    {
        From = from;
        To = to;
    }
}
