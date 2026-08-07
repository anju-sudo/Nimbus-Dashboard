using MediatR;

namespace NimbusBoard.Application.Boards;

public record DeleteBoardCommand(Guid BoardId) : IRequest<Unit>;
