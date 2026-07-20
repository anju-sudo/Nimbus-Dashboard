using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Label : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";

    public Project Project { get; set; } = null!;
    public ICollection<IssueLabel> IssueLabels { get; set; } = [];
}
