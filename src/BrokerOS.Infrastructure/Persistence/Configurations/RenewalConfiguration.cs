using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

/// <summary>
/// Renewal workflow mapping. RenewalDate is SQL date (DateOnly). Follow-up columns stay datetime2 (UTC).
/// Restrict deletes so completing or losing a renewal cannot remove the policy term it belongs to.
/// </summary>
public sealed class RenewalConfiguration : IEntityTypeConfiguration<Renewal>
{
    public void Configure(EntityTypeBuilder<Renewal> builder)
    {
        builder.ToTable("Renewals");
        builder.ConfigurePublicId();
        builder.ConfigureAuditStrings();

        builder.Property(entity => entity.RenewalDate).HasColumnType("date").IsRequired();
        builder.EnumAsString(entity => entity.Status);
        builder.EnumAsString(entity => entity.Priority);
        builder.EnumAsString(entity => entity.CurrentStage);
        builder.Property(entity => entity.Notes).HasMaxLength(FieldLengths.Notes);

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Renewals)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Policy)
            .WithMany(policy => policy.Renewals)
            .HasForeignKey(entity => entity.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.RenewalDate });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Status });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.AssignedUserId });
    }
}
