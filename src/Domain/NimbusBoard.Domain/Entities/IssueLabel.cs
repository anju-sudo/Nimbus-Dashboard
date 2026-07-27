namespace NimbusBoard.Domain.Entities;

public class IssueLabel
{
    public Guid IssueId { get; set; }
    public Guid LabelId { get; set; }

    public Issue Issue { get; set; } = null!;
    public Label Label { get; set; } = null!;
}
