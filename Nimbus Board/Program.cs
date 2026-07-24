using NimbusBoard.Infrastructure;

WebApplicationBuilder builder = WebApplication.CreateBuilder(args);

// Resolve |DataDirectory| for SQLite (same as Umbraco)
var dataDirectory = Path.Combine(builder.Environment.ContentRootPath, "umbraco", "Data");
Directory.CreateDirectory(dataDirectory);
AppDomain.CurrentDomain.SetData("DataDirectory", dataDirectory);

builder.CreateUmbracoBuilder()
    .AddBackOffice()
    .AddWebsite()
    .AddComposers()
    .Build();

WebApplication app = builder.Build();

await app.BootUmbracoAsync();
await app.Services.EnsureNimbusBoardDatabaseAsync();

app.UseUmbraco()
    .WithMiddleware(u =>
    {
        u.UseBackOffice();
        u.UseWebsite();
    })
    .WithEndpoints(u =>
    {
        u.UseBackOfficeEndpoints();
        u.UseWebsiteEndpoints();
    });

app.MapRazorPages();
app.MapHub<Nimbus_Board.Hubs.NotificationHub>("/app/hubs/notifications");

await app.RunAsync();
