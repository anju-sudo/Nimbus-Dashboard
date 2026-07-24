using NimbusBoard.Application.Common.Interfaces;
using Nimbus_Board.Services;
using NimbusBoard.Infrastructure;
using NimbusBoard.Infrastructure.Services;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;

namespace Nimbus_Board.Composers;

public class NimbusBoardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var connectionString = builder.Config.GetConnectionString("NimbusBoard")
            ?? "Data Source=|DataDirectory|/NimbusBoard.sqlite.db;Cache=Shared";

        builder.Services.AddNimbusBoardInfrastructure(connectionString);
        builder.Services.Configure<SmtpOptions>(builder.Config.GetSection(SmtpOptions.SectionName));
        builder.Services.AddScoped<LocalFileAttachmentStorage>();
        builder.Services.AddScoped<UmbracoMediaAttachmentAdapter>();
        builder.Services.AddScoped<IAttachmentStorage, UmbracoMediaAttachmentAdapter>();
        builder.Services.AddScoped<IAppNotificationService, SignalRNotificationPublisher>();
        builder.Services.AddSignalR();
        builder.Services.AddRazorPages();
    }
}
