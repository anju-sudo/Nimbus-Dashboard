using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Board : BaseEntity
{
    public Guid ProjectId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string BoardType { get; set; } = "Kanban";

    public Project Project { get; set; } = null!;
    public ICollection<BoardColumn> Columns { get; set; } = [];
}
