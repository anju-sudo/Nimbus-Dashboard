using MediatR;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Nimbus_Board.Models;
using NimbusBoard.Application.Dashboard;
using Umbraco.Cms.Core.PublishedCache;
using Umbraco.Cms.Core.Services.Navigation;

namespace Nimbus_Board.Pages.App;

public class DashboardModel(
    IMediator mediator,
    IDocumentNavigationQueryService navigationQuery,
    IPublishedContentCache contentCache) : PageModel
{
    public DashboardViewModel ViewModel { get; private set; } = new();
    public DashboardCopy Copy { get; private set; } = DashboardCopy.Defaults;

    public async Task OnGetAsync()
    {
        ViewModel = await mediator.Send(new GetDashboardQuery());
        Copy = await ResolveCopyAsync();

        ViewData["Title"] = Copy.PageTitle;
        ViewData["ActiveNav"] = "dashboard";
        ViewData["UnreadNotifications"] = ViewModel.UnreadNotifications;
        ViewData["UserName"] = "Anjumol Babu";
        ViewData["UserInitials"] = ViewModel.UserInitials;
        ViewData["WorkspaceName"] = ViewModel.WorkspaceName;
        ViewData["MemberId"] = 1;
        ViewData["BrandName"] = Copy.BrandName;
    }

    private async Task<DashboardCopy> ResolveCopyAsync()
    {
        if (!navigationQuery.TryGetRootKeysOfType("home", out var keys))
        {
            return DashboardCopy.Defaults;
        }

        var key = keys.FirstOrDefault();
        if (key == Guid.Empty)
        {
            return DashboardCopy.Defaults;
        }

        var home = await contentCache.GetByIdAsync(key);
        return DashboardCopy.From(home);
    }
}
