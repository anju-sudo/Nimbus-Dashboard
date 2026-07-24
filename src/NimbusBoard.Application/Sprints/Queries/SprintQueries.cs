using MediatR;
using NimbusBoard.Application.Sprints.Models;

namespace NimbusBoard.Application.Sprints.Queries;

public record GetSprintsQuery(Guid? ProjectId = null) : IRequest<IReadOnlyList<SprintListItemViewModel>>;

public record GetSprintDetailQuery(Guid SprintId) : IRequest<SprintDetailViewModel?>;

public record GetSprintCreateFormQuery : IRequest<SprintCreateFormViewModel>;
