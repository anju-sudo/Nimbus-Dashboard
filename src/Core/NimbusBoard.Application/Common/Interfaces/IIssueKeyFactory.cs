using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Application.Common.Interfaces;

public interface IIssueKeyFactory
{
    Task<(int Number, string Key)> CreateNextKeyAsync(Project project, CancellationToken cancellationToken = default);
}
