using MediatR;
using NimbusBoard.Application.Projects;

namespace NimbusBoard.Application.Projects;

public record GetProjectsQuery : IRequest<IReadOnlyList<ProjectListItemViewModel>>;

public record GetProjectByKeyQuery(string Key) : IRequest<ProjectDetailViewModel?>;

public record GetDefaultWorkspaceIdQuery : IRequest<Guid>;
