using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

/// <summary>Brokerage tenant. Code is globally unique (used at registration). Query filter is Id == current org.</summary>
public sealed class OrganizationConfiguration : IEntityTypeConfiguration<Organization>
{
    public void Configure(EntityTypeBuilder<Organization> builder)
    {
        builder.ToTable("Organizations");
        builder.ConfigurePublicId();

        builder.Property(entity => entity.Name).IsRequired().HasMaxLength(FieldLengths.Name);
        builder.Property(entity => entity.Code).IsRequired().HasMaxLength(FieldLengths.Code);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();

        builder.HasIndex(entity => entity.Code).IsUnique();
    }
}
