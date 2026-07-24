namespace NimbusBoard.Application.Collaboration.Models;

public class CommentViewModel
{
    public Guid Id { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public string AuthorInitials { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}

public class AttachmentViewModel
{
    public Guid Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string UploadedBy { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}

public class LabelViewModel
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Color { get; set; } = "#6366f1";
    public bool IsAssigned { get; set; }
}

public class IssueActivityViewModel
{
    public string Message { get; set; } = string.Empty;
    public string TimeAgo { get; set; } = string.Empty;
}
