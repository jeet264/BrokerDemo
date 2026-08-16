using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// A person who can sign in to a brokerage. Users are tenant-owned: the same email
/// cannot be reused across organizations while the account is not deleted.
/// Role drives both HTTP policies and AssignmentScope (employees see only their assigned book).
/// </summary>
public class User : Entity, ITenantOwned, IAudited, ISoftDeletable
{
    /// <summary>Brokerage this login belongs to. Copied into the JWT as OrganizationId — the source of tenant scope.</summary>
    public long OrganizationId { get; set; }

    /// <summary>Unique among non-deleted users (global unique index). Normalized to lowercase on write.</summary>
    public string Email { get; set; } = string.Empty;

    public string FullName { get; set; } = string.Empty;

    /// <summary>BrokerAdmin = full org; BrokerManager = operations without org-settings; BrokerEmployee = assigned rows only.</summary>
    public UserRole Role { get; set; } = UserRole.BrokerEmployee;

    /// <summary>ASP.NET Identity password hash. Never returned in DTOs.</summary>
    public string PasswordHash { get; set; } = string.Empty;

    /// <summary>False blocks login even if the password is correct. Distinct from <see cref="IsDeleted"/> (soft-removed account).</summary>
    public bool IsActive { get; set; } = true;

    /// <summary>Last successful login. UTC DateTime (audit), not a calendar DateOnly.</summary>
    public DateTime? LastLoginAtUtc { get; set; }

    public DateTime CreatedAtUtc { get; set; }

    public DateTime? ModifiedAtUtc { get; set; }

    public string? CreatedBy { get; set; }

    public string? ModifiedBy { get; set; }

    public bool IsDeleted { get; set; }

    public Organization Organization { get; set; } = null!;
}
