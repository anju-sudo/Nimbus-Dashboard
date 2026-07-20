using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using NimbusBoard.Application.Dashboard.Models;
using NimbusBoard.Application.Dashboard.Queries;

namespace Nimbus_Board.Pages.App;

public class DashboardModel(IMediator mediator) : PageModel
{
    public DashboardViewModel ViewModel { get; private set; } = new();

    public async Task OnGetAsync()
    {
        ViewModel = await mediator.Send(new GetDashboardQuery());
        ViewData["Title"] = "Dashboard";
        ViewData["ActiveNav"] = "dashboard";
        ViewData["UnreadNotifications"] = ViewModel.UnreadNotifications;
        ViewData["UserName"] = ViewModel.UserName + " Silva";
        ViewData["UserInitials"] = ViewModel.UserInitials;
        ViewData["WorkspaceName"] = ViewModel.WorkspaceName;
    }
}
