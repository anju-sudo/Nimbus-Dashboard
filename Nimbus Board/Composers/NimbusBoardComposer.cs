using NimbusBoard.Infrastructure;
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
        builder.Services.AddSignalR();
        builder.Services.AddRazorPages();
    }
}
