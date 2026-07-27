using NimbusBoard.Application.Common.Interfaces;

namespace Nimbus_Board.Services;

/// <summary>
/// Fallback storage when Umbraco media is unavailable (e.g. during early boot).
/// </summary>
public class LocalFileAttachmentStorage(IWebHostEnvironment env) : IAttachmentStorage
{
    private static int _nextId = 10000;

    public async Task<int> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default)
    {
        var id = Interlocked.Increment(ref _nextId);
        var dir = Path.Combine(env.WebRootPath, "nimbus-uploads", id.ToString());
        Directory.CreateDirectory(dir);
        var path = Path.Combine(dir, Path.GetFileName(fileName));
        await using var fs = File.Create(path);
        await fileStream.CopyToAsync(fs, cancellationToken);
        return id;
    }

    public string GetMediaUrl(int mediaId)
    {
        var dir = Path.Combine(env.WebRootPath, "nimbus-uploads", mediaId.ToString());
        if (!Directory.Exists(dir))
        {
            return "#";
        }

        var file = Directory.GetFiles(dir).FirstOrDefault();
        return file is null ? "#" : $"/nimbus-uploads/{mediaId}/{Path.GetFileName(file)}";
    }

    public Task DeleteAsync(int mediaId, CancellationToken cancellationToken = default)
    {
        var dir = Path.Combine(env.WebRootPath, "nimbus-uploads", mediaId.ToString());
        if (Directory.Exists(dir))
        {
            Directory.Delete(dir, true);
        }

        return Task.CompletedTask;
    }
}
