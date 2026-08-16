namespace BrokerOS.Domain.Activities;

/// <summary>
/// Title helpers for a desk quick-capture note. This is not NLP — it only takes the first line
/// so a follow-up task has something short to show on the task list.
/// </summary>
public static class QuickNoteText
{
    public const int FollowUpTitleMaxLength = 80;

    public static string FollowUpTitle(string text)
    {
        var trimmed = text.Trim();
        if (trimmed.Length == 0)
        {
            return "Follow up";
        }

        var newline = trimmed.IndexOfAny(['\r', '\n']);
        var firstLine = newline >= 0 ? trimmed[..newline].Trim() : trimmed;
        if (firstLine.Length == 0)
        {
            return "Follow up";
        }

        if (firstLine.Length <= FollowUpTitleMaxLength)
        {
            return firstLine;
        }

        return firstLine[..FollowUpTitleMaxLength].TrimEnd() + "…";
    }
}
