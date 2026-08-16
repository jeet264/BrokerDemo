namespace BrokerOS.Application.MyDay;

/// <summary>
/// One next action for the logged-in broker. Built so the card can be acted on without opening a detail page:
/// names, phone, policy number, and the suggested actions travel with the row.
/// </summary>
public sealed class MyDayItemDto
{
    public required MyDayItemKind Kind { get; init; }

    /// <summary>PublicId of the underlying Renewal or WorkTask.</summary>
    public required Guid PublicId { get; init; }

    public Guid? ClientPublicId { get; init; }

    public string? ClientName { get; init; }

    /// <summary>For the Call button (tel:). Null when the client has no phone on file.</summary>
    public string? ClientPhone { get; init; }

    public Guid? PolicyPublicId { get; init; }

    public string? PolicyNumber { get; init; }

    /// <summary>Plain-English next action, e.g. "Call Sunrise Textiles — motor cover expired 12 days ago".</summary>
    public required string ActionNeeded { get; init; }

    public required MyDayBucket Bucket { get; init; }

    /// <summary>IST calendar date this item is due on (cover date or follow-up/task date converted to IST).</summary>
    public required DateOnly DueOn { get; init; }

    /// <summary>Set only for overdue items. today − DueOn in IST days.</summary>
    public int? DaysOverdue { get; init; }

    /// <summary>Enum name of renewal/task priority.</summary>
    public required string Priority { get; init; }

    /// <summary>Renewal stage name when Kind is Renewal; otherwise null.</summary>
    public string? Stage { get; init; }

    public required IReadOnlyList<MyDayAction> AvailableActions { get; init; }
}
