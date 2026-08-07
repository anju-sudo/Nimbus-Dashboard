using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence.Configurations;

public sealed class BurndownSnapshotConfiguration : IEntityTypeConfiguration<BurndownSnapshot>
{
    public void Configure(EntityTypeBuilder<BurndownSnapshot> builder)
    {
        builder.HasOne(b => b.Sprint).WithMany(s => s.BurndownSnapshots).HasForeignKey(b => b.SprintId);
    }
}
