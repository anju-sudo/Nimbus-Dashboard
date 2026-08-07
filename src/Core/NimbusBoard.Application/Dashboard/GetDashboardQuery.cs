using MediatR;
using NimbusBoard.Application.Dashboard;

namespace NimbusBoard.Application.Dashboard;

public record GetDashboardQuery(int? MemberId = null) : IRequest<DashboardViewModel>;
