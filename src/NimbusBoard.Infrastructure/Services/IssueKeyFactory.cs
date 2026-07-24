using NimbusBoard.Application.Common.Interfaces;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Services;

public class IssueKeyFactory(INimbusBoardDbContext db) : IIssueKeyFactory
{
    public async Task<(int Number, string Key)> CreateNextKeyAsync(Project project, CancellationToken cancellationToken = default)
    {
        project.IssueCounter++;
        var number = project.IssueCounter;
        var key = $"{project.Key}-{number}";
        await db.SaveChangesAsync(cancellationToken);
        return (number, key);
    }
}
