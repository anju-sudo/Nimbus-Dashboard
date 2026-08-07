using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence.Configurations;

public sealed class IssueLabelConfiguration : IEntityTypeConfiguration<IssueLabel>
{
    public void Configure(EntityTypeBuilder<IssueLabel> builder)
    {
        builder.HasKey(il => new { il.IssueId, il.LabelId });
        builder.HasOne(il => il.Issue).WithMany(i => i.IssueLabels).HasForeignKey(il => il.IssueId);
        builder.HasOne(il => il.Label).WithMany(l => l.IssueLabels).HasForeignKey(il => il.LabelId);
    }
}
