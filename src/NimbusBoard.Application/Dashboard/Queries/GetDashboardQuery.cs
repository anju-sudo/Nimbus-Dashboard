using MediatR;
using NimbusBoard.Application.Dashboard.Models;

namespace NimbusBoard.Application.Dashboard.Queries;

public record GetDashboardQuery(int? MemberId = null) : IRequest<DashboardViewModel>;
