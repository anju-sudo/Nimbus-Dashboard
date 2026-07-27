using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Common;

public static class IssueStatusMapper
{
    public static IssueStatus FromColumnName(string columnName) => columnName.ToLowerInvariant() switch
    {
        "backlog" => IssueStatus.Backlog,
        "to do" => IssueStatus.ToDo,
        "in progress" => IssueStatus.InProgress,
        "review" => IssueStatus.Review,
        "done" => IssueStatus.Done,
        _ => IssueStatus.ToDo
    };

    public static string ToDisplayName(IssueStatus status) => status switch
    {
        IssueStatus.Backlog => "Backlog",
        IssueStatus.ToDo => "To Do",
        IssueStatus.InProgress => "In Progress",
        IssueStatus.Review => "Review",
        IssueStatus.Done => "Done",
        _ => status.ToString()
    };
}
