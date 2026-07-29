using MediatR;

namespace NimbusBoard.Application.Projects.Commands;

public record CreateProjectResult(Guid Id, string Key);

public record CreateProjectCommand(
    string Key,
    string Name,
    string? Description,
    Guid WorkspaceId) : IRequest<CreateProjectResult>;

public record AddProjectMemberCommand(
    Guid ProjectId,
    string DisplayName,
    string Initials,
    string Role) : IRequest<Guid>;
