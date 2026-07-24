using MediatR;

namespace NimbusBoard.Application.Sprints.Commands;

public record CreateSprintCommand(
    Guid ProjectId,
    string Name,
    string? Goal,
    DateTime StartDate,
    DateTime EndDate) : IRequest<Guid>;

public record StartSprintCommand(Guid SprintId, int ActorMemberId = 1, string ActorName = "Anjumol Babu") : IRequest<Unit>;

public record CompleteSprintCommand(Guid SprintId, int ActorMemberId = 1, string ActorName = "Anjumol Babu") : IRequest<Unit>;

public record AssignIssueToSprintCommand(Guid IssueId, Guid? SprintId) : IRequest<Unit>;
