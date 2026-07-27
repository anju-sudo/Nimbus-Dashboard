using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class BoardColumn : BaseEntity
{
    public Guid BoardId { get; set; }
    public string Name { get; set; } = string.Empty;
    public int SortOrder { get; set; }
    public int? WipLimit { get; set; }

    public Board Board { get; set; } = null!;
    public ICollection<Issue> Issues { get; set; } = [];
}
