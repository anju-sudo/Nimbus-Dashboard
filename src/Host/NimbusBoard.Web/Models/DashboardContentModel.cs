using NimbusBoard.Application.Dashboard;

namespace Nimbus_Board.Models;

public sealed class DashboardContentModel
{
    public required DashboardViewModel Data { get; init; }
    public required DashboardCopy Copy { get; init; }
}
