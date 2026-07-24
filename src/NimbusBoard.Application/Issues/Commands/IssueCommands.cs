using MediatR;
using NimbusBoard.Application.Issues.Models;

namespace NimbusBoard.Application.Issues.Commands;

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
    string? AssigneeName,
    string? AssigneeInitials,
    int? AssigneeMemberId = 1) : IRequest<string>;

public record UpdateIssueCommand(
    string Key,
    string Title,
    string? Description,
    string Type,
    string Priority,
    int? StoryPoints,
    DateTime? DueDate,
    string? AssigneeName,
    string? AssigneeInitials) : IRequest<Unit>;

public record MoveIssueCommand(
    Guid IssueId,
    Guid BoardColumnId,
    int SortOrder = 0) : IRequest<Unit>;
