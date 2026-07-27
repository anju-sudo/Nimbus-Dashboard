using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Common;

public static class IssueStatusStateMachine
{
    private static readonly Dictionary<IssueStatus, HashSet<IssueStatus>> Allowed = new()
    {
        [IssueStatus.Backlog] = [IssueStatus.ToDo, IssueStatus.InProgress],
        [IssueStatus.ToDo] = [IssueStatus.Backlog, IssueStatus.InProgress, IssueStatus.Done],
        [IssueStatus.InProgress] = [IssueStatus.ToDo, IssueStatus.Review, IssueStatus.Done],
        [IssueStatus.Review] = [IssueStatus.InProgress, IssueStatus.Done, IssueStatus.ToDo],
        [IssueStatus.Done] = [IssueStatus.ToDo, IssueStatus.InProgress, IssueStatus.Review]
    };

    public static bool CanTransition(IssueStatus from, IssueStatus to)
    {
        if (from == to)
        {
            return true;
        }

        return Allowed.TryGetValue(from, out var targets) && targets.Contains(to);
    }

    public static void EnsureCanTransition(IssueStatus from, IssueStatus to)
    {
        if (!CanTransition(from, to))
        {
            throw new InvalidOperationException($"Cannot move issue from {from} to {to}.");
        }
    }
}
