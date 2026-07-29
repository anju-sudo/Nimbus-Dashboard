using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using NimbusBoard.Application.Common.Interfaces;
using Nimbus_Board.Hubs;
using Nimbus_Board.Notifications;
using Nimbus_Board.Services;
using NimbusBoard.Infrastructure;
using NimbusBoard.Infrastructure.Email;
using NimbusBoard.Infrastructure.Notifications;
using NimbusBoard.Infrastructure.Storage;
using Umbraco.Cms.Core.Composing;
using Umbraco.Cms.Core.DependencyInjection;
using Umbraco.Cms.Core.Notifications;

namespace Nimbus_Board.Composers;

public sealed class NimbusBoardComposer : IComposer
{
    public void Compose(IUmbracoBuilder builder)
    {
        var connectionString = builder.Config.GetConnectionString("NimbusBoard")
            ?? "Data Source=|DataDirectory|/NimbusBoard.sqlite.db;Cache=Shared";

        builder.Services.AddNimbusBoardInfrastructure(connectionString);
        builder.Services.Configure<SmtpOptions>(builder.Config.GetSection(SmtpOptions.SectionName));
        builder.Services.AddOptions<AttachmentStorageOptions>().Configure<IWebHostEnvironment>((opts, env) =>
        {
            opts.RootPath = Path.Combine(env.WebRootPath, "nimbus-uploads");
            opts.PublicPathPrefix = "/nimbus-uploads";
        });

        builder.Services.AddScoped<UmbracoMediaAttachmentAdapter>();
        builder.Services.AddScoped<IAttachmentStorage>(sp => sp.GetRequiredService<UmbracoMediaAttachmentAdapter>());
        builder.Services.AddScoped<IAppNotificationService>(sp =>
            new SignalRNotificationPublisher(
                sp.GetRequiredService<NotificationPublisher>(),
                sp.GetRequiredService<Microsoft.AspNetCore.SignalR.IHubContext<NotificationHub>>()));

        builder.Services.AddSignalR();
        builder.Services.AddRazorPages();
        builder.AddNotificationAsyncHandler<UmbracoApplicationStartedNotification, HomeContentSeedNotificationHandler>();
    }
}
