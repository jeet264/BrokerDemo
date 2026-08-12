using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

public sealed class PolicyConfiguration : IEntityTypeConfiguration<Policy>
{
    public void Configure(EntityTypeBuilder<Policy> builder)
    {
        builder.ToTable("Policies");
        builder.ConfigurePublicId();
        builder.ConfigureAuditStrings();

        builder.Property(entity => entity.PolicyNumber).IsRequired().HasMaxLength(FieldLengths.PolicyNumber);
        builder.EnumAsString(entity => entity.PolicyType);
        builder.Property(entity => entity.StartDate).HasColumnType("date").IsRequired();
        builder.Property(entity => entity.ExpiryDate).HasColumnType("date").IsRequired();
        builder.Property(entity => entity.Premium).HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.SumInsured).HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.CommissionPercentage).HasPrecision(18, 4).IsRequired();
        builder.Property(entity => entity.CommissionAmount).HasPrecision(18, 2).IsRequired();
        builder.EnumAsString(entity => entity.Status);
        builder.Property(entity => entity.Notes).HasMaxLength(FieldLengths.Notes);
        builder.Property(entity => entity.IsDeleted).IsRequired();

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Policies)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Client)
            .WithMany(client => client.Policies)
            .HasForeignKey(entity => entity.ClientId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Insurer)
            .WithMany(insurer => insurer.Policies)
            .HasForeignKey(entity => entity.InsurerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.AssignedUser)
            .WithMany()
            .HasForeignKey(entity => entity.AssignedUserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.ExpiryDate });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.ClientId });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.InsurerId });
        builder.HasIndex(entity => new { entity.OrganizationId, entity.PolicyNumber })
            .IsUnique()
            .HasFilter("[IsDeleted] = 0");
    }
}
