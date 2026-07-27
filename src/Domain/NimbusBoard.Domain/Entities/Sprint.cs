using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Sprint : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Goal { get; set; }
    public DateTime StartDate { get; set; }
    public DateTime EndDate { get; set; }
    public bool IsActive { get; set; }
    public int TotalStoryPoints { get; set; }
    public int CompletedStoryPoints { get; set; }

    public Project Project { get; set; } = null!;
    public ICollection<Issue> Issues { get; set; } = [];
    public ICollection<BurndownSnapshot> BurndownSnapshots { get; set; } = [];
}
