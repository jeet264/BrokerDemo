namespace BrokerOS.Domain.Common;

/// <summary>Optional marker when a type exposes <c>PublicId</c> without inheriting <see cref="Entity"/>.</summary>
public interface IHasPublicId
{
    Guid PublicId { get; set; }
}

/// <summary>
/// Row belongs to one brokerage. EF Core query filters match this to
/// <c>ITenantContext.OrganizationId</c> (copied from the JWT — never from the request body).
/// </summary>
public interface ITenantOwned
{
    long OrganizationId { get; set; }
}

/// <summary>
/// Soft-delete marker. <c>DbContext.SaveChanges</c> converts <c>EntityState.Deleted</c>
/// into <c>IsDeleted = true</c> so history is retained. Query filters hide these rows.
/// Renewal and Activity do not implement this: they are workflow / audit records, not address-book rows.
/// </summary>
public interface ISoftDeletable
{
    bool IsDeleted { get; set; }
}

/// <summary>
/// UTC audit columns. <c>CreatedAtUtc</c> / <c>ModifiedAtUtc</c> are timestamps, not cover dates —
/// do not change them to <see cref="DateOnly"/>. <c>CreatedBy</c> / <c>ModifiedBy</c> store the
/// current user's <c>PublicId</c> (or email fallback) from tenant context.
/// </summary>
public interface IAudited
{
    DateTime CreatedAtUtc { get; set; }

    DateTime? ModifiedAtUtc { get; set; }

    string? CreatedBy { get; set; }

    string? ModifiedBy { get; set; }
}
