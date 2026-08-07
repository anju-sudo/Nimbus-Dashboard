using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using NimbusBoard.Domain.Entities;

namespace NimbusBoard.Infrastructure.Persistence.Configurations;

public sealed class AttachmentConfiguration : IEntityTypeConfiguration<Attachment>
{
    public void Configure(EntityTypeBuilder<Attachment> builder)
    {
        builder.HasOne(a => a.Issue).WithMany(i => i.Attachments).HasForeignKey(a => a.IssueId);
    }
}
