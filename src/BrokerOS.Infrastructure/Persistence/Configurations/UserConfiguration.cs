using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");
        builder.ConfigurePublicId();
        builder.ConfigureAuditStrings();

        builder.Property(entity => entity.Email).IsRequired().HasMaxLength(FieldLengths.Email);
        builder.Property(entity => entity.FullName).IsRequired().HasMaxLength(FieldLengths.FullName);
        builder.Property(entity => entity.PasswordHash).IsRequired().HasMaxLength(FieldLengths.PasswordHash);
        builder.EnumAsString(entity => entity.Role);
        builder.Property(entity => entity.IsActive).IsRequired();
        builder.Property(entity => entity.IsDeleted).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Users)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => entity.OrganizationId);
        builder.HasIndex(entity => entity.Email)
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
