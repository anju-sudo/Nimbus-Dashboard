using Microsoft.Extensions.Options;
using NimbusBoard.Application.Common.Abstractions;

namespace NimbusBoard.Infrastructure.Services;

/// <summary>
/// Filesystem-backed attachment storage. Used as the default Infrastructure
/// implementation and as a fallback from the Host Umbraco media adapter.
/// </summary>
public sealed class LocalFileAttachmentStorage(IOptions<AttachmentStorageOptions> options) : IAttachmentStorage
{
    private static int _nextId = 10000;
    private readonly AttachmentStorageOptions _options = options.Value;

    public async Task<int> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        EnsureRootConfigured();
        var id = Interlocked.Increment(ref _nextId);
        var dir = Path.Combine(_options.RootPath, id.ToString());
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, Path.GetFileName(fileName));
        await using var fs = File.Create(path);
        await fileStream.CopyToAsync(fs, cancellationToken);
        return id;
    }

    public string GetMediaUrl(int mediaId)
    {
        EnsureRootConfigured();
        var dir = Path.Combine(_options.RootPath, mediaId.ToString());
        if (!Directory.Exists(dir))
        {
            return "#";
        }

        var file = Directory.GetFiles(dir).FirstOrDefault();
        return file is null
            ? "#"
            : $"{_options.PublicPathPrefix.TrimEnd('/')}/{mediaId}/{Path.GetFileName(file)}";
    }

    public Task DeleteAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        EnsureRootConfigured();
        var dir = Path.Combine(_options.RootPath, mediaId.ToString());
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }

        return Task.CompletedTask;
    }

    private void EnsureRootConfigured()
    {
        if (string.IsNullOrWhiteSpace(_options.RootPath))
        {
            throw new InvalidOperationException(
                "AttachmentStorage:RootPath is not configured. The Host must set it to a writable folder (e.g. wwwroot/nimbus-uploads).");
        }

        Directory.CreateDirectory(_options.RootPath);
    }
}
