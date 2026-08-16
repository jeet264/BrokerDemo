using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

/// <summary>Client book. ClientCode unique per org among non-deleted rows. AssignedUserId null = unassigned.</summary>
public sealed class ClientConfiguration : IEntityTypeConfiguration<Client>
{
    public void Configure(EntityTypeBuilder<Client> builder)
    {
        builder.ToTable("Clients");
        builder.ConfigurePublicId();
        builder.ConfigureAuditStrings();

        builder.Property(entity => entity.ClientCode).IsRequired().HasMaxLength(FieldLengths.ClientCode);
        builder.Property(entity => entity.CompanyName).IsRequired().HasMaxLength(FieldLengths.Name);
        builder.EnumAsString(entity => entity.ClientType);
        builder.Property(entity => entity.Industry).HasMaxLength(FieldLengths.Industry);
        builder.Property(entity => entity.Email).IsRequired().HasMaxLength(FieldLengths.Email);
        builder.Property(entity => entity.Phone).IsRequired().HasMaxLength(FieldLengths.Phone);
        builder.Property(entity => entity.AlternatePhone).HasMaxLength(FieldLengths.Phone);
        builder.Property(entity => entity.AddressLine1).IsRequired().HasMaxLength(FieldLengths.Address);
        builder.Property(entity => entity.AddressLine2).HasMaxLength(FieldLengths.Address);
        builder.Property(entity => entity.City).IsRequired().HasMaxLength(FieldLengths.City);
        builder.Property(entity => entity.State).IsRequired().HasMaxLength(FieldLengths.State);
        builder.Property(entity => entity.PostalCode).IsRequired().HasMaxLength(FieldLengths.PostalCode);
        builder.Property(entity => entity.Country).IsRequired().HasMaxLength(FieldLengths.Country);
        builder.Property(entity => entity.Notes).HasMaxLength(FieldLengths.Notes);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.IsDeleted).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Clients)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.CompanyName });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.ClientCode })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
