using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

public sealed class InsurerConfiguration : IEntityTypeConfiguration<Insurer>
{
    public void Configure(EntityTypeBuilder<Insurer> builder)
    {
        builder.ToTable("Insurers");
        builder.ConfigurePublicId();

        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(FieldLengths.Name);
        builder.Property(entity => entity.Code).IsRequired().HasMaxLength(FieldLengths.Code);
        builder.Property(entity => entity.Email).HasMaxLength(FieldLengths.Email);
        builder.Property(entity => entity.Phone).HasMaxLength(FieldLengths.Phone);
        builder.Property(entity => entity.Website).HasMaxLength(FieldLengths.Url);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Insurers)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict)
            .IsRequired(false);

        builder.HasIndex(entity => entity.OrganizationId);
        builder.HasIndex(entity => new { entity.OrganizationId, entity.Code })
            .IsUnique()
            .HasFilter("[OrganizationId] IS NOT NULL");
        builder.HasIndex(entity => entity.Code)
            .IsUnique()
            .HasFilter("[OrganizationId] IS NULL");
    }
}
