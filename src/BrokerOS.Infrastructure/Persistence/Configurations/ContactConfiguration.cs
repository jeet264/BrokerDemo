using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

/// <summary>People at a client. Soft-deleted with the address book. Restrict on Client so contacts cannot orphan.</summary>
public sealed class ContactConfiguration : IEntityTypeConfiguration<Contact>
{
    public void Configure(EntityTypeBuilder<Contact> builder)
    {
        builder.ToTable("Contacts");
        builder.ConfigurePublicId();

        builder.Property(entity => entity.FirstName).IsRequired().HasMaxLength(FieldLengths.Name);
        builder.Property(entity => entity.LastName).IsRequired().HasMaxLength(FieldLengths.Name);
        builder.Property(entity => entity.Designation).HasMaxLength(FieldLengths.Designation);
        builder.Property(entity => entity.Email).IsRequired().HasMaxLength(FieldLengths.Email);
        builder.Property(entity => entity.Phone).IsRequired().HasMaxLength(FieldLengths.Phone);
        builder.Property(entity => entity.IsPrimary).IsRequired();
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
        builder.Property(entity => entity.IsDeleted).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Contacts)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Client)
            .WithMany(client => client.Contacts)
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.ClientId });
    }
}
