using MediatR;
using NimbusBoard.Application.Sprints;

namespace NimbusBoard.Application.Sprints;

public record GetSprintsQuery(Guid? ProjectId = null) : IRequest<IReadOnlyList<SprintListItemViewModel>>;

public record GetSprintDetailQuery(Guid SprintId) : IRequest<SprintDetailViewModel?>;

public record GetSprintCreateFormQuery : IRequest<SprintCreateFormViewModel>;
