using BrokerOS.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence.Configurations;

public sealed class QuotationConfiguration : IEntityTypeConfiguration<Quotation>
{
    public void Configure(EntityTypeBuilder<Quotation> builder)
    {
        builder.ToTable("Quotations");
        builder.ConfigurePublicId();
        builder.ConfigureAuditStrings();

        builder.Property(entity => entity.PremiumAmount).HasPrecision(18, 2).IsRequired();
        builder.Property(entity => entity.SumInsured).HasPrecision(18, 2);
        builder.Property(entity => entity.CoverageSummary).IsRequired().HasMaxLength(FieldLengths.CoverageSummary);
        builder.Property(entity => entity.ValidUntil).HasColumnType("date");
        builder.EnumAsString(entity => entity.Status);
        builder.Property(entity => entity.Notes).HasMaxLength(FieldLengths.Notes);

        builder.HasOne(entity => entity.Organization)
            .WithMany(organization => organization.Quotations)
            .HasForeignKey(entity => entity.OrganizationId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Renewal)
            .WithMany(renewal => renewal.Quotations)
            .HasForeignKey(entity => entity.RenewalId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(entity => entity.Insurer)
            .WithMany(insurer => insurer.Quotations)
            .HasForeignKey(entity => entity.InsurerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(entity => new { entity.OrganizationId, entity.RenewalId });
        builder.HasIndex(entity => new { entity.RenewalId, entity.Status });
    }
}
