using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

/// <summary>Append-only activity. No IsDeleted column — timeline rows are not removed.</summary>
public sealed class ActivityConfiguration : IEntityTypeConfiguration<Activity>
{
    public void Configure(EntityTypeBuilder<Activity> builder)
    {
        builder.ToTable("Activities");
        builder.ConfigurePublicId();

        builder.EnumAsString(entity => entity.ActivityType);
        builder.Property(entity => entity.Description).IsRequired().HasMaxLength(FieldLengths.Description);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Activities)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Client)
            .WithMany(client => client.Activities)
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Policy)
            .WithMany(policy => policy.Activities)
            .HasForeignKey(entity => entity.PolicyId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Renewal)
            .WithMany(renewal => renewal.Activities)
            .HasForeignKey(entity => entity.RenewalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.User)
            .WithMany()
            .HasForeignKey(entity => entity.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.CreatedAtUtc });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.ClientId });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.PolicyId });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.RenewalId });
    }
}
