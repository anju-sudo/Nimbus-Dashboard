using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Application.Dashboard.Models;

namespace NimbusBoard.Application.Sprints;

public static class BurndownQueryHelper
{
    public static async Task<BurndownChartViewModel> BuildAsync(
        INimbusBoardDbContext db,
        Guid? sprintId,
        CancellationToken cancellationToken = default)
    {
        if (sprintId is null)
        {
            return new BurndownChartViewModel();
        }

        var snapshots = await db.BurndownSnapshots
            .Where(b => b.SprintId == sprintId)
            .OrderBy(b => b.SnapshotDate)
            .ToListAsync(cancellationToken);

        return new BurndownChartViewModel
        {
            Labels = snapshots.Select(s => s.SnapshotDate.ToString("MMM d")).ToList(),
            Actual = snapshots.Select(s => s.RemainingPoints).ToList(),
            Ideal = snapshots.Select(s => s.IdealPoints).ToList()
        };
    }
}
