using MediatR;
using NimbusBoard.Application.Issues.Models;

namespace NimbusBoard.Application.Issues.Queries;

public record GetIssueByKeyQuery(string Key) : IRequest<IssueDetailViewModel?>;

public record GetMyWorkQuery(int MemberId = 1) : IRequest<IReadOnlyList<IssueListItemViewModel>>;

public record GetIssueCreateFormQuery(Guid ProjectId, Guid? BoardColumnId = null) : IRequest<CreateIssueFormModel?>;
