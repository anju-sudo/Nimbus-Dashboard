using Microsoft.Extensions.Logging;
using Umbraco.Cms.Core;
using Umbraco.Cms.Core.Events;
using Umbraco.Cms.Core.Models;
using Umbraco.Cms.Core.Notifications;
using Umbraco.Cms.Core.Services;
using Umbraco.Cms.Core.Strings;

namespace Nimbus_Board.Notifications;

/// <summary>
/// Ensures the Home document type has editable dashboard label properties
/// and that a published root Home node exists so / serves the FE.
/// </summary>
public sealed class HomeContentSeedNotificationHandler(
    IContentTypeService contentTypeService,
    IDataTypeService dataTypeService,
    IContentService contentService,
    IShortStringHelper shortStringHelper,
    ILogger<HomeContentSeedNotificationHandler> logger)
    : INotificationAsyncHandler<UmbracoApplicationStartedNotification>
{
    private static readonly (string Alias, string Name, string DefaultValue)[] Properties =
    [
        ("pageTitle", "Page title", "Dashboard"),
        ("greetingPrefix", "Greeting prefix", "Good morning"),
        ("searchPlaceholder", "Search placeholder", "Search..."),
        ("newIssueLabel", "New issue button", "+ New Issue"),
        ("kpiTotalProjects", "KPI · Projects", "Projects"),
        ("kpiOpenIssues", "KPI · Open issues", "Open issues"),
        ("kpiInProgress", "KPI · In progress", "In progress"),
        ("kpiDoneThisSprint", "KPI · Done this sprint", "Done this sprint"),
        ("kpiOverdue", "KPI · Overdue", "Overdue"),
        ("kpiUrgent", "KPI · Urgent", "Urgent"),
        ("sectionUrgentTasks", "Section · Urgent tasks", "Urgent tasks"),
        ("sectionRecentActivity", "Section · Recent activity", "Recent activity"),
        ("viewAllLabel", "View all link", "View all"),
        ("emptyUrgentTasks", "Empty · Urgent tasks", "No urgent tasks."),
        ("emptyRecentActivity", "Empty · Recent activity", "No recent activity."),
        ("brandName", "Brand name", "Nimbus"),
    ];

    public async Task HandleAsync(UmbracoApplicationStartedNotification notification, CancellationToken cancellationToken)
    {
        try
        {
            await EnsureHomePropertiesAsync();
            EnsureRootHomeContent();
        }
        catch (Exception ex)
        {
            logger.LogError(ex, "Failed to seed Umbraco Home content for the Nimbus dashboard.");
        }
    }

    private async Task EnsureHomePropertiesAsync()
    {
        var homeType = contentTypeService.Get("home");
        if (homeType is null)
        {
            logger.LogWarning("Umbraco content type 'home' was not found; dashboard CMS labels were not seeded.");
            return;
        }

        var textstring = dataTypeService.GetDataType("Textstring")
            ?? dataTypeService.GetByEditorAlias(Constants.PropertyEditors.Aliases.TextBox).FirstOrDefault();

        if (textstring is null)
        {
            logger.LogWarning("Umbraco Textstring data type was not found; dashboard CMS labels were not seeded.");
            return;
        }

        var added = false;
        foreach (var (alias, name, _) in Properties)
        {
            if (homeType.PropertyTypeExists(alias))
            {
                continue;
            }

            var propertyType = new PropertyType(shortStringHelper, textstring, alias)
            {
                Name = name,
                Description = "Editable dashboard label used by the Nimbus home template.",
                Mandatory = false,
                SortOrder = homeType.PropertyTypes.Count() + 1,
            };

            homeType.AddPropertyType(propertyType, "dashboardCopy", "Dashboard copy");
            added = true;
        }

        if (!added)
        {
            return;
        }

        homeType.AllowedAsRoot = true;
        await contentTypeService.UpdateAsync(homeType, Constants.Security.SuperUserKey);
        logger.LogInformation("Added CMS dashboard copy properties to the Home document type.");
    }

    private void EnsureRootHomeContent()
    {
        var existing = contentService.GetRootContent()
            .FirstOrDefault(c => c.ContentType.Alias.Equals("home", StringComparison.OrdinalIgnoreCase));

        if (existing is not null)
        {
            var dirty = false;
            foreach (var (alias, _, defaultValue) in Properties)
            {
                if (existing.HasProperty(alias)
                    && string.IsNullOrWhiteSpace(existing.GetValue<string>(alias)))
                {
                    existing.SetValue(alias, defaultValue);
                    dirty = true;
                }
            }

            if (dirty)
            {
                contentService.Save(existing);
                contentService.Publish(existing, ["*"]);
                logger.LogInformation("Filled empty Home CMS labels with defaults.");
            }

            return;
        }

        var home = contentService.Create("Home", Constants.System.Root, "home");
        foreach (var (alias, _, defaultValue) in Properties)
        {
            if (home.HasProperty(alias))
            {
                home.SetValue(alias, defaultValue);
            }
        }

        contentService.Save(home);
        contentService.Publish(home, ["*"]);
        logger.LogInformation("Created and published root Home content for /.");
    }
}
