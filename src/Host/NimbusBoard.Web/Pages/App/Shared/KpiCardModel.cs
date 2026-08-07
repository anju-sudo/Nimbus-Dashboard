namespace Nimbus_Board.Pages.App.Shared;

public enum KpiIcon
{
    Folder,
    List,
    Progress,
    Done,
    Warning,
    Flame
}

public record KpiCardModel(
    string Label,
    int Value,
    string BackgroundClass,
    string IconClass,
    KpiIcon Icon);
