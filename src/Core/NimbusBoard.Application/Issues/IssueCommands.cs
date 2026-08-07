using MediatR;
using NimbusBoard.Application.Issues;

namespace NimbusBoard.Application.Issues;

public record CreateIssueResult(string Key, Guid? BoardId);

public record CreateIssueCommand(
    Guid ProjectId,
    string Title,
    string? Description,
    string Type,
    string Priority,
    Guid? BoardColumnId,
    Guid? SprintId,
    int? StoryPoints,
    DateTime? DueDate,
    int? AssigneeMemberId = null) : IRequest<CreateIssueResult>;

public record UpdateIssueCommand(
    string Key,
    string Title,
    string? Description,
    string Type,
    string Priority,
    int? StoryPoints,
    DateTime? DueDate,
    int? AssigneeMemberId) : IRequest<Unit>;

public record MoveIssueCommand(
    Guid IssueId,
    Guid BoardColumnId,
    int SortOrder = 0) : IRequest<Unit>;
