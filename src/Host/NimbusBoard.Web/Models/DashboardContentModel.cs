using NimbusBoard.Application.Dashboard.Models;

namespace Nimbus_Board.Models;

public sealed class DashboardContentModel
{
    public required DashboardViewModel Data { get; init; }
    public required DashboardCopy Copy { get; init; }
}
