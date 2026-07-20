using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class BurndownSnapshot : BaseEntity
{
    public Guid SprintId { get; set; }
    public DateTime SnapshotDate { get; set; }
    public int RemainingPoints { get; set; }
    public int IdealPoints { get; set; }

    public Sprint Sprint { get; set; } = null!;
}
