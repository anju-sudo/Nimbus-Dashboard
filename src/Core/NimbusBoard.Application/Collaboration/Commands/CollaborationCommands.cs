using MediatR;

namespace NimbusBoard.Application.Collaboration.Commands;

public record AddCommentCommand(
    string IssueKey,
    string Body,
    int AuthorMemberId = 1,
    string AuthorName = "Anjumol Babu") : IRequest<Guid>;

public record UploadAttachmentCommand(
    string IssueKey,
    Stream FileStream,
    string FileName,
    int UploadedByMemberId = 1,
    string UploadedByName = "Anjumol Babu") : IRequest<Guid>;

public record DeleteAttachmentCommand(Guid AttachmentId) : IRequest<Unit>;

public record CreateLabelCommand(
    Guid ProjectId,
    string Name,
    string Color) : IRequest<Guid>;

public record ToggleIssueLabelCommand(
    string IssueKey,
    Guid LabelId,
    string ActorName = "Anjumol Babu") : IRequest<bool>;
