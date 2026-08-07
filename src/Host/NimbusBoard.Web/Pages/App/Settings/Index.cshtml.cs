using MediatR;
using Microsoft.Extensions.Options;
using NimbusBoard.Application.Boards;
using NimbusBoard.Application.Common.Abstractions;
using NimbusBoard.Infrastructure.Services;

namespace Nimbus_Board.Pages.App.Settings;

public class IndexModel(
    IMediator mediator,
    IEmailSender emailSender,
    IOptions<SmtpOptions> smtpOptions) : AppPageModel
{
    public bool SmtpEnabled { get; private set; }
    public string SmtpHost { get; private set; } = "localhost";
    public string SmtpFrom { get; private set; } = "nimbus@localhost";
    public IReadOnlyList<BoardListItemViewModel> Boards { get; private set; } = [];

    public async Task OnGetAsync()
    {
        await SetLayoutDataAsync("settings", "Settings");
        var options = smtpOptions.Value;
        SmtpEnabled = emailSender.IsEnabled;
        SmtpHost = options.Host;
        SmtpFrom = options.From;
        Boards = await mediator.Send(new GetBoardsQuery());
    }
}
