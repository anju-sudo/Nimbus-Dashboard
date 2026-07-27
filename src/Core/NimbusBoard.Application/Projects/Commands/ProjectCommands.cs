using MediatR;

namespace NimbusBoard.Application.Projects.Commands;

public record CreateProjectCommand(
    string Key,
    string Name,
    string? Description,
    Guid WorkspaceId) : IRequest<Guid>;

public record AddProjectMemberCommand(
    Guid ProjectId,
    string DisplayName,
    string Initials,
    string Role) : IRequest<Guid>;
