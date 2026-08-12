using System.Linq.Expressions;
using BrokerOS.Domain.Common;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace BrokerOS.Infrastructure.Persistence;

internal static class EntityTypeBuilderExtensions
{
    public static void ConfigurePublicId<T>(this EntityTypeBuilder<T> builder)
        where T : Entity
    {
        builder.HasKey(entity => entity.Id);
        builder.Property(entity => entity.Id).UseIdentityColumn();
        builder.Property(entity => entity.PublicId).IsRequired();
        builder.HasIndex(entity => entity.PublicId).IsUnique();
    }

    public static void ConfigureAuditStrings<T>(this EntityTypeBuilder<T> builder)
        where T : class, IAudited
    {
        builder.Property(entity => entity.CreatedBy).HasMaxLength(FieldLengths.AuditUser);
        builder.Property(entity => entity.ModifiedBy).HasMaxLength(FieldLengths.AuditUser);
        builder.Property(entity => entity.CreatedAtUtc).IsRequired();
    }

    public static PropertyBuilder<TEnum> EnumAsString<TEntity, TEnum>(
        this EntityTypeBuilder<TEntity> builder,
        Expression<Func<TEntity, TEnum>> propertyExpression)
        where TEntity : class
        where TEnum : struct, Enum
    {
        return builder.Property(propertyExpression)
            .HasConversion<string>()
            .HasMaxLength(FieldLengths.Enum)
            .IsRequired();
    }
}
