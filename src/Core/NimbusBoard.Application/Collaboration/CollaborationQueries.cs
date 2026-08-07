using MediatR;
using NimbusBoard.Application.Collaboration;

namespace NimbusBoard.Application.Collaboration;

public record GetIssueCommentsQuery(string IssueKey) : IRequest<IReadOnlyList<CommentViewModel>?>;

public record GetIssueActivityQuery(string IssueKey) : IRequest<IReadOnlyList<IssueActivityViewModel>?>;
