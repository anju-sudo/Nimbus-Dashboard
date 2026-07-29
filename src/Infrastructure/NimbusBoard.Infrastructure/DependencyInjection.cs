using Microsoft.Extensions.DependencyInjection;
using NimbusBoard.Application;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Infrastructure.Burndown;
using NimbusBoard.Infrastructure.Email;
using NimbusBoard.Infrastructure.Identity;
using NimbusBoard.Infrastructure.Notifications;
using NimbusBoard.Infrastructure.Persistence;
using NimbusBoard.Infrastructure.Persistence.Seeding;
using NimbusBoard.Infrastructure.Storage;
using Microsoft.EntityFrameworkCore;

namespace NimbusBoard.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddNimbusBoardInfrastructure(this IServiceCollection services, string connectionString)
    {
        services.AddDbContext<NimbusBoardDbContext>(options =>
            options.UseSqlite(connectionString));

        services.AddScoped<INimbusBoardDbContext>(sp => sp.GetRequiredService<NimbusBoardDbContext>());
        services.AddScoped<IIssueKeyFactory, IssueKeyFactory>();
        services.AddScoped<IBurndownService, BurndownService>();
        services.AddScoped<IEmailSender, SmtpEmailSender>();
        services.AddScoped<NotificationPublisher>();
        services.AddScoped<IAppNotificationService>(sp => sp.GetRequiredService<NotificationPublisher>());
        services.AddScoped<LocalFileAttachmentStorage>();
        services.AddScoped<IAttachmentStorage>(sp => sp.GetRequiredService<LocalFileAttachmentStorage>());
        services.AddOptions<SmtpOptions>();
        services.AddOptions<AttachmentStorageOptions>();
        services.AddNimbusBoardApplication();

        return services;
    }

    public static Task EnsureNimbusBoardDatabaseAsync(this IServiceProvider services) =>
        NimbusBoardSeeder.EnsureDatabaseAsync(services);
}
