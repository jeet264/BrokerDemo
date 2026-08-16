using FluentValidation;

namespace BrokerOS.Application.QuickNotes;

public sealed class CreateQuickNoteRequestValidator : AbstractValidator<CreateQuickNoteRequest>
{
    public CreateQuickNoteRequestValidator()
    {
        RuleFor(request => request.Text)
            .NotEmpty()
            .WithMessage("Note text is required")
            .MaximumLength(2000);

        RuleFor(request => request.TaskDueDateUtc)
            .Empty()
            .When(request => !request.CreateFollowUpTask)
            .WithMessage("Due date is only used when creating a follow-up task.");
    }
}
