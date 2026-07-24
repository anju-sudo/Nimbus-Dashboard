namespace NimbusBoard.Application.Notifications.Models;

public class NotificationItemViewModel
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public string? LinkUrl { get; set; }
    public bool IsRead { get; set; }
    public string TimeAgo { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}
