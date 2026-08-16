namespace BrokerOS.Application.QuickNotes;

/// <summary>
/// Desk capture: free text plus optional links. Public APIs use PublicId (GUID), never the
/// internal long ClientId / RenewalId.
/// </summary>
public sealed class CreateQuickNoteRequest
{
    public string Text { get; set; } = string.Empty;

    public Guid? ClientPublicId { get; set; }

    public Guid? RenewalPublicId { get; set; }

    /// <summary>
    /// When true, also insert a Task. The API does not infer this from the wording.
    /// </summary>
    public bool CreateFollowUpTask { get; set; }

    public DateTime? TaskDueDateUtc { get; set; }
}
