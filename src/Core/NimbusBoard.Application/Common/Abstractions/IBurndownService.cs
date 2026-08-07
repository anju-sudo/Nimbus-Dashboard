namespace NimbusBoard.Application.Common.Abstractions;

public interface IBurndownService
{
    Task RecalculateSprintPointsAsync(Guid sprintId, CancellationToken cancellationToken = default);
    Task TakeSnapshotAsync(Guid sprintId, DateTime? asOfDate = null, CancellationToken cancellationToken = default);
    Task EnsureTodaySnapshotAsync(Guid sprintId, CancellationToken cancellationToken = default);
}
