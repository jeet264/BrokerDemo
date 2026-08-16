using BrokerOS.Domain.Activities;

namespace BrokerOS.Domain.Tests;

public sealed class QuickNoteTextTests
{
    [Fact]
    public void FollowUpTitle_uses_first_line()
    {
        Assert.Equal("Call back after 4pm", QuickNoteText.FollowUpTitle("Call back after 4pm\nThey want a quote."));
    }

    [Fact]
    public void FollowUpTitle_truncates_long_lines()
    {
        var text = new string('a', 90);
        var title = QuickNoteText.FollowUpTitle(text);

        Assert.Equal(81, title.Length);
        Assert.EndsWith("…", title);
    }

    [Fact]
    public void FollowUpTitle_falls_back_when_blank()
    {
        Assert.Equal("Follow up", QuickNoteText.FollowUpTitle("   \n  "));
    }
}
