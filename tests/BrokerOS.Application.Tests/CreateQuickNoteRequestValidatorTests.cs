using BrokerOS.Application.QuickNotes;

namespace BrokerOS.Application.Tests;

public sealed class CreateQuickNoteRequestValidatorTests
{
    private readonly CreateQuickNoteRequestValidator _validator = new();

    [Fact]
    public void Text_is_required()
    {
        var result = _validator.Validate(new CreateQuickNoteRequest { Text = "  " });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "Text");
    }

    [Fact]
    public void Unlinked_note_without_task_is_valid()
    {
        var result = _validator.Validate(new CreateQuickNoteRequest { Text = "Called, will send quote tomorrow." });

        Assert.True(result.IsValid);
    }

    [Fact]
    public void Due_date_without_task_flag_is_rejected()
    {
        var result = _validator.Validate(new CreateQuickNoteRequest
        {
            Text = "Need a callback",
            CreateFollowUpTask = false,
            TaskDueDateUtc = DateTime.UtcNow.AddDays(1)
        });

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, error => error.PropertyName == "TaskDueDateUtc");
    }
}
