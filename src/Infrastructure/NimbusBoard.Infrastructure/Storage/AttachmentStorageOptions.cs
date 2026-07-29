namespace NimbusBoard.Infrastructure.Storage;

public sealed class AttachmentStorageOptions
{
    public const string SectionName = "AttachmentStorage";

    /// <summary>
    /// Absolute path to the folder that stores locally uploaded attachments.
    /// Typically {WebRoot}/nimbus-uploads when configured from the Host.
    /// </summary>
    public string RootPath { get; set; } = string.Empty;

    /// <summary>
    /// Public URL prefix for local files (e.g. /nimbus-uploads).
    /// </summary>
    public string PublicPathPrefix { get; set; } = "/nimbus-uploads";
}
