using MediatR;
using NimbusBoard.Application.Collaboration.Models;

namespace NimbusBoard.Application.Collaboration.Queries;

public record GetIssueCommentsQuery(string IssueKey) : IRequest<IReadOnlyList<CommentViewModel>?>;

public record GetIssueActivityQuery(string IssueKey) : IRequest<IReadOnlyList<IssueActivityViewModel>?>;
