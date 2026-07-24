using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;
using NimbusBoard.Infrastructure.Persistence;

namespace NimbusBoard.Infrastructure.Services;

public class BurndownService(NimbusBoardDbContext db) : IBurndownService
{
    public async Task RecalculateSprintPointsAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        var sprint = await db.Sprints
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint is null)
        {
            return;
        }

        sprint.TotalStoryPoints = sprint.Issues.Sum(i => Math.Max(0, i.StoryPoints ?? 0));
        sprint.CompletedStoryPoints = sprint.Issues
            .Where(i => i.Status == IssueStatus.Done)
            .Sum(i => Math.Max(0, i.StoryPoints ?? 0));
        sprint.UpdatedAt = DateTime.UtcNow;

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task TakeSnapshotAsync(Guid sprintId, DateTime? asOfDate = null, CancellationToken cancellationToken = default)
    {
        var sprint = await db.Sprints
            .Include(s => s.Issues)
            .FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);

        if (sprint is null)
        {
            return;
        }

        var date = (asOfDate ?? DateTime.UtcNow).Date;
        var remaining = BurndownCalculator.CalculateRemainingPoints(
            sprint.Issues.Select(i => (i.StoryPoints ?? 0, i.Status == IssueStatus.Done)));
        var ideal = BurndownCalculator.CalculateIdealPoints(
            sprint.TotalStoryPoints > 0 ? sprint.TotalStoryPoints : sprint.Issues.Sum(i => Math.Max(0, i.StoryPoints ?? 0)),
            sprint.StartDate,
            sprint.EndDate,
            date);

        var existing = await db.BurndownSnapshots
            .FirstOrDefaultAsync(s => s.SprintId == sprintId && s.SnapshotDate == date, cancellationToken);

        if (existing is null)
        {
            db.BurndownSnapshots.Add(new BurndownSnapshot
            {
                SprintId = sprintId,
                SnapshotDate = date,
                RemainingPoints = remaining,
                IdealPoints = ideal
            });
        }
        else
        {
            existing.RemainingPoints = remaining;
            existing.IdealPoints = ideal;
            existing.UpdatedAt = DateTime.UtcNow;
        }

        await db.SaveChangesAsync(cancellationToken);
    }

    public async Task EnsureTodaySnapshotAsync(Guid sprintId, CancellationToken cancellationToken = default)
    {
        var sprint = await db.Sprints.FirstOrDefaultAsync(s => s.Id == sprintId, cancellationToken);
        if (sprint is null)
        {
            return;
        }

        var today = DateTime.UtcNow.Date;
        var snapshotCount = await db.BurndownSnapshots.CountAsync(s => s.SprintId == sprintId, cancellationToken);

        // Backfill from sprint start when the chart has no history yet.
        if (snapshotCount == 0)
        {
            await RecalculateSprintPointsAsync(sprintId, cancellationToken);
            var end = today < sprint.EndDate.Date ? today : sprint.EndDate.Date;
            if (end < sprint.StartDate.Date)
            {
                end = sprint.StartDate.Date;
            }

            for (var day = sprint.StartDate.Date; day <= end; day = day.AddDays(1))
            {
                await TakeSnapshotAsync(sprintId, day, cancellationToken);
            }

            return;
        }

        var existsToday = await db.BurndownSnapshots
            .AnyAsync(s => s.SprintId == sprintId && s.SnapshotDate == today, cancellationToken);

        if (!existsToday && today >= sprint.StartDate.Date)
        {
            await RecalculateSprintPointsAsync(sprintId, cancellationToken);
            await TakeSnapshotAsync(sprintId, today, cancellationToken);
        }
    }
}
