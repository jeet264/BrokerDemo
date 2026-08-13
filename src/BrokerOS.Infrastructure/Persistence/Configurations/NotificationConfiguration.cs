using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

public sealed class NotificationConfiguration : IEntityTypeConfiguration<Notification>
{
    public void Configure(EntityTypeBuilder<Notification> builder)
    {
        builder.ToTable("Notifications");
        builder.ConfigurePublicId();

        builder.EnumAsString(entity => entity.RecipientType);
        builder.EnumAsString(entity => entity.Channel);
        builder.EnumAsString(entity => entity.Status);
        builder.Property(entity => entity.Subject).IsRequired().HasMaxLength(FieldLengths.Title);
        builder.Property(entity => entity.Body).IsRequired().HasMaxLength(FieldLengths.Description);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Notifications)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Renewal)
            .WithMany(renewal => renewal.Notifications)
            .HasForeignKey(entity => entity.RenewalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Client)
            .WithMany(client => client.Notifications)
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.CreatedAtUtc });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.RenewalId });
        builder.HasIndex(entity => new { entity.RenewalId, entity.ReminderMilestoneDays })
            .IsUnique()
            .HasFilter("[ReminderMilestoneDays] IS NOT NULL");
    }
}
