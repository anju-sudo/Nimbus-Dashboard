using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Application.Common.Abstractions;

public interface IIssueKeyFactory
{
    Task<(int Number, string Key)> CreateNextKeyAsync(Project project, CancellationToken cancellationToken = default);
}
