using MediatR;
using NimbusBoard.Application.Boards.Models;

namespace NimbusBoard.Application.Boards.Queries;

public record GetBoardQuery(Guid BoardId) : IRequest<BoardViewModel?>;

public record GetBoardsQuery : IRequest<IReadOnlyList<BoardListItemViewModel>>;
