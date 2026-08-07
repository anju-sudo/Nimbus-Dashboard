using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence.Configurations;

public sealed class IssueConfiguration : IEntityTypeConfiguration<Issue>
{
    public void Configure(EntityTypeBuilder<Issue> builder)
    {
        builder.HasIndex(i => i.Key).IsUnique();
        builder.HasOne(i => i.Project).WithMany(p => p.Issues).HasForeignKey(i => i.ProjectId);
        builder.HasOne(i => i.BoardColumn).WithMany(c => c.Issues).HasForeignKey(i => i.BoardColumnId);
        builder.HasOne(i => i.Sprint).WithMany(s => s.Issues).HasForeignKey(i => i.SprintId);
    }
}
