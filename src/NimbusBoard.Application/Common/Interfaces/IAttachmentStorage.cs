namespace NimbusBoard.Application.Common.Interfaces;

public interface IAttachmentStorage
{
    Task<int> SaveAsync(Stream fileStream, string fileName, CancellationToken cancellationToken = default);
    string GetMediaUrl(int mediaId);
    Task DeleteAsync(int mediaId, CancellationToken cancellationToken = default);
}
