using Microsoft.AspNetCore.SignalR;

namespace Nimbus_Board.Hubs;

public class NotificationHub : Hub
{
    public override async Task OnConnectedAsync()
    {
        var memberId = Context.GetHttpContext()?.Request.Query["memberId"].FirstOrDefault() ?? "1";
        await Groups.AddToGroupAsync(Context.ConnectionId, $"member:{memberId}");
        await base.OnConnectedAsync();
    }

    public Task JoinMemberGroup(int memberId) =>
        Groups.AddToGroupAsync(Context.ConnectionId, $"member:{memberId}");
}
