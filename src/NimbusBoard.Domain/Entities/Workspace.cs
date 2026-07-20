using NimbusBoard.Domain.Common;

namespace NimbusBoard.Domain.Entities;

public class Workspace : BaseEntity
{
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;

    public ICollection<Project> Projects { get; set; } = [];
}
