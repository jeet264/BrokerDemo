using BrokerOS.Domain.Enums;
using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class CreateFollowUpRequestValidator : AbstractValidator<CreateFollowUpRequest>
{
    private static readonly ActivityType[] AllowedTypes =
    [
        ActivityType.Note,
        ActivityType.Call,
        ActivityType.Email,
        ActivityType.WhatsApp,
        ActivityType.ClientContact,
        ActivityType.InsurerContact,
        ActivityType.Meeting
    ];

    public CreateFollowUpRequestValidator()
    {
        RuleFor(request => request.ActivityType)
            .IsInEnum()
            .Must(type => AllowedTypes.Contains(type))
            .WithMessage("Follow-up activity type is not supported.");

        RuleFor(request => request.Description)
            .NotEmpty()
            .WithMessage("Description is required")
            .MaximumLength(2000);

        RuleFor(request => request.TaskTitle)
            .MaximumLength(200);
    }
}
