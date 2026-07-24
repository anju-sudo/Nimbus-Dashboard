using MediatR;
using NimbusBoard.Application.Projects.Models;

namespace NimbusBoard.Application.Projects.Queries;

public record GetProjectsQuery : IRequest<IReadOnlyList<ProjectListItemViewModel>>;

public record GetProjectByKeyQuery(string Key) : IRequest<ProjectDetailViewModel?>;
