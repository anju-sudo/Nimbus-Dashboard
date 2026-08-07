using MediatR;
using NimbusBoard.Application.Search;

namespace NimbusBoard.Application.Search;

public record SearchQuery(string Term, int Limit = 10) : IRequest<SearchResultsViewModel>;
