using MediatR;
using NimbusBoard.Application.Search.Models;

namespace NimbusBoard.Application.Search.Queries;

public record SearchQuery(string Term, int Limit = 10) : IRequest<SearchResultsViewModel>;
