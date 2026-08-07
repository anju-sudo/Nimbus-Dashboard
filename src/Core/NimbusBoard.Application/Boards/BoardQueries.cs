using MediatR;
using NimbusBoard.Application.Boards;

namespace NimbusBoard.Application.Boards;

public record GetBoardQuery(Guid BoardId) : IRequest<BoardViewModel?>;

public record GetBoardsQuery : IRequest<IReadOnlyList<BoardListItemViewModel>>;
