using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

public sealed class WorkTaskConfiguration : IEntityTypeConfiguration<WorkTask>
{
    public void Configure(EntityTypeBuilder<WorkTask> builder)
    {
        builder.ToTable("Tasks");
        builder.ConfigurePublicId();
        builder.ConfigureAuditStrings();

        builder.Property(entity => entity.Title).IsRequired().HasMaxLength(FieldLengths.Title);
        builder.Property(entity => entity.Description).HasMaxLength(FieldLengths.Description);
        builder.Property(entity => entity.DueDateUtc).IsRequired();
        builder.EnumAsString(entity => entity.Priority);
        builder.EnumAsString(entity => entity.Status);
        builder.Property(entity => entity.IsDeleted).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Tasks)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Renewal)
            .WithMany(renewal => renewal.Tasks)
            .HasForeignKey(entity => entity.RenewalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Client)
            .WithMany(client => client.Tasks)
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Policy)
            .WithMany(policy => policy.Tasks)
            .HasForeignKey(entity => entity.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.DueDateUtc });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Status });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.AssignedUserId });
        builder.HasIndex(entity => new { entity.RenewalId, entity.ReminderMilestoneDays })
            .IsUnique()
            .HasFilter("[RenewalId] IS NOT NULL AND [ReminderMilestoneDays] IS NOT NULL AND [IsDeleted] = 0");
    }
}
