using BrokerOS.Domain.Common;
using BrokerOS.Domain.Enums;

namespace BrokerOS.Domain.Entities;

/// <summary>
/// An append-only timeline entry (note, call, email, status change, and similar).
/// Optional FKs attach the event to a client, policy, and/or renewal; <see cref="UserId"/> is required
/// so we always know who recorded it. Not <see cref="ISoftDeletable"/> and not <see cref="IAudited"/>:
/// activities are not edited — they only have <see cref="CreatedAtUtc"/>.
/// </summary>
public class Activity : Entity, ITenantOwned
{
    public long OrganizationId { get; set; }

    /// <summary>Null when the activity is not about a client (e.g. a system note on a policy only).</summary>
    public long? ClientId { get; set; }

    /// <summary>Null when the activity is not about a specific policy term.</summary>
    public long? PolicyId { get; set; }

    /// <summary>Null when the activity is not part of a renewal workflow.</summary>
    public long? RenewalId { get; set; }

    /// <summary>The user who recorded the activity (required). Not the same as "assigned to".</summary>
    public long UserId { get; set; }

    public ActivityType ActivityType { get; set; }

    public string Description { get; set; } = string.Empty;

    /// <summary>When the event was recorded. UTC DateTime (audit), not a business DateOnly.</summary>
    public DateTime CreatedAtUtc { get; set; }

    public Organization Organization { get; set; } = null!;

    public Client? Client { get; set; }

    public Policy? Policy { get; set; }

    public Renewal? Renewal { get; set; }

    public User User { get; set; } = null!;
}
