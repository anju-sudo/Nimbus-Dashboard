using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence.Configurations;

public sealed class LabelConfiguration : IEntityTypeConfiguration<Label>
{
    public void Configure(EntityTypeBuilder<Label> builder)
    {
        builder.HasOne(l => l.Project).WithMany().HasForeignKey(l => l.ProjectId);
    }
}
