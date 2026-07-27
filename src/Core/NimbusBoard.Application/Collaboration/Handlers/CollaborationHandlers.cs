using MediatR;
using Microsoft.EntityFrameworkCore;
using NimbusBoard.Application.Collaboration.Commands;
using NimbusBoard.Application.Collaboration.Models;
using NimbusBoard.Application.Common;
using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;
using NimbusBoard.Domain.Enums;

namespace NimbusBoard.Application.Collaboration.Handlers;

public class AddCommentCommandHandler(
    INimbusBoardDbContext db,
    IAppNotificationService notifications) : IRequestHandler<AddCommentCommand, Guid>
{
    public async Task<Guid> Handle(AddCommentCommand request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.Key == request.IssueKey, cancellationToken)
            ?? throw new InvalidOperationException("Issue not found.");

        var comment = new Comment
        {
            IssueId = issue.Id,
            AuthorMemberId = request.AuthorMemberId,
            AuthorName = request.AuthorName,
            Body = request.Body.Trim()
        };

        db.Comments.Add(comment);
        db.ActivityLogs.Add(new ActivityLog
        {
            IssueId = issue.Id,
            ProjectId = issue.ProjectId,
            ActorMemberId = request.AuthorMemberId,
            ActorName = request.AuthorName,
            Action = "commented on",
            Detail = issue.Key
        });

        await db.SaveChangesAsync(cancellationToken);

        var recipient = issue.AssigneeMemberId ?? request.AuthorMemberId;
        if (recipient != request.AuthorMemberId || issue.AssigneeMemberId is null)
        {
            await notifications.PublishAsync(
                recipient,
                NotificationType.Commented,
                $"{request.AuthorName} commented on {issue.Key}",
                $"/app/issues/{issue.Key}",
                issue.Id,
                cancellationToken: cancellationToken);
        }

        return comment.Id;
    }
}

public class UploadAttachmentCommandHandler(
    INimbusBoardDbContext db,
    IAttachmentStorage storage) : IRequestHandler<UploadAttachmentCommand, Guid>
{
    public async Task<Guid> Handle(UploadAttachmentCommand request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues.FirstOrDefaultAsync(i => i.Key == request.IssueKey, cancellationToken)
            ?? throw new InvalidOperationException("Issue not found.");

        var mediaId = await storage.SaveAsync(request.FileStream, request.FileName, cancellationToken);

        var attachment = new Attachment
        {
            IssueId = issue.Id,
            MediaId = mediaId,
            FileName = request.FileName,
            UploadedByMemberId = request.UploadedByMemberId
        };

        db.Attachments.Add(attachment);
        db.ActivityLogs.Add(new ActivityLog
        {
            IssueId = issue.Id,
            ProjectId = issue.ProjectId,
            ActorMemberId = request.UploadedByMemberId,
            ActorName = request.UploadedByName,
            Action = "attached",
            Detail = request.FileName
        });

        await db.SaveChangesAsync(cancellationToken);
        return attachment.Id;
    }
}

public class DeleteAttachmentCommandHandler(
    INimbusBoardDbContext db,
    IAttachmentStorage storage) : IRequestHandler<DeleteAttachmentCommand, Unit>
{
    public async Task<Unit> Handle(DeleteAttachmentCommand request, CancellationToken cancellationToken)
    {
        var attachment = await db.Attachments
            .Include(a => a.Issue)
            .FirstOrDefaultAsync(a => a.Id == request.AttachmentId, cancellationToken)
            ?? throw new InvalidOperationException("Attachment not found.");

        await storage.DeleteAsync(attachment.MediaId, cancellationToken);
        db.Attachments.Remove(attachment);

        db.ActivityLogs.Add(new ActivityLog
        {
            IssueId = attachment.IssueId,
            ProjectId = attachment.Issue.ProjectId,
            ActorMemberId = attachment.UploadedByMemberId,
            ActorName = "User",
            Action = "removed attachment",
            Detail = attachment.FileName
        });

        await db.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}

public class CreateLabelCommandHandler(INimbusBoardDbContext db) : IRequestHandler<CreateLabelCommand, Guid>
{
    public async Task<Guid> Handle(CreateLabelCommand request, CancellationToken cancellationToken)
    {
        var label = new Label
        {
            ProjectId = request.ProjectId,
            Name = request.Name.Trim(),
            Color = request.Color
        };

        db.Labels.Add(label);
        await db.SaveChangesAsync(cancellationToken);
        return label.Id;
    }
}

public class ToggleIssueLabelCommandHandler(INimbusBoardDbContext db) : IRequestHandler<ToggleIssueLabelCommand, bool>
{
    public async Task<bool> Handle(ToggleIssueLabelCommand request, CancellationToken cancellationToken)
    {
        var issue = await db.Issues
            .Include(i => i.IssueLabels)
            .FirstOrDefaultAsync(i => i.Key == request.IssueKey, cancellationToken)
            ?? throw new InvalidOperationException("Issue not found.");

        var existing = issue.IssueLabels.FirstOrDefault(il => il.LabelId == request.LabelId);
        var isNowAssigned = existing is null;

        if (existing is not null)
        {
            db.IssueLabels.Remove(existing);
        }
        else
        {
            db.IssueLabels.Add(new IssueLabel { IssueId = issue.Id, LabelId = request.LabelId });
        }

        var label = await db.Labels.FirstOrDefaultAsync(l => l.Id == request.LabelId, cancellationToken)
            ?? throw new InvalidOperationException("Label not found.");
        db.ActivityLogs.Add(new ActivityLog
        {
            IssueId = issue.Id,
            ProjectId = issue.ProjectId,
            ActorMemberId = 1,
            ActorName = request.ActorName,
            Action = isNowAssigned ? "added label" : "removed label",
            Detail = label.Name
        });

        await db.SaveChangesAsync(cancellationToken);
        return isNowAssigned;
    }
}

public static class CollaborationQueryHelper
{
    public static async Task<IReadOnlyList<CommentViewModel>> GetCommentsAsync(
        INimbusBoardDbContext db, Guid issueId, CancellationToken ct)
    {
        var comments = await db.Comments
            .Where(c => c.IssueId == issueId)
            .OrderByDescending(c => c.CreatedAt)
            .ToListAsync(ct);

        return comments.Select(c => new CommentViewModel
        {
            Id = c.Id,
            AuthorName = c.AuthorName,
            AuthorInitials = GetInitials(c.AuthorName),
            Body = c.Body,
            TimeAgo = TimeAgoHelper.Format(c.CreatedAt)
        }).ToList();
    }

    public static async Task<IReadOnlyList<AttachmentViewModel>> GetAttachmentsAsync(
        INimbusBoardDbContext db, IAttachmentStorage storage, Guid issueId, CancellationToken ct)
    {
        var attachments = await db.Attachments
            .Where(a => a.IssueId == issueId)
            .OrderByDescending(a => a.CreatedAt)
            .ToListAsync(ct);

        return attachments.Select(a => new AttachmentViewModel
        {
            Id = a.Id,
            FileName = a.FileName,
            Url = storage.GetMediaUrl(a.MediaId),
            UploadedBy = "Member",
            TimeAgo = TimeAgoHelper.Format(a.CreatedAt)
        }).ToList();
    }

    public static async Task<IReadOnlyList<LabelViewModel>> GetProjectLabelsAsync(
        INimbusBoardDbContext db, Guid projectId, Guid issueId, CancellationToken ct)
    {
        var assignedIds = await db.IssueLabels
            .Where(il => il.IssueId == issueId)
            .Select(il => il.LabelId)
            .ToListAsync(ct);

        return await db.Labels
            .Where(l => l.ProjectId == projectId)
            .OrderBy(l => l.Name)
            .Select(l => new LabelViewModel
            {
                Id = l.Id,
                Name = l.Name,
                Color = l.Color,
                IsAssigned = assignedIds.Contains(l.Id)
            })
            .ToListAsync(ct);
    }

    public static async Task<IReadOnlyList<IssueActivityViewModel>> GetActivityAsync(
        INimbusBoardDbContext db, Guid issueId, CancellationToken ct)
    {
        var logs = await db.ActivityLogs
            .Where(a => a.IssueId == issueId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(20)
            .ToListAsync(ct);

        return logs.Select(a => new IssueActivityViewModel
        {
            Message = $"{a.ActorName} {a.Action}{(a.Detail is not null ? " " + a.Detail : "")}",
            TimeAgo = TimeAgoHelper.Format(a.CreatedAt)
        }).ToList();
    }

    private static string GetInitials(string name)
    {
        var parts = name.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 0)
        {
            return "??";
        }

        return string.Concat(parts.Take(2).Select(p => char.ToUpperInvariant(p[0])));
    }
}
