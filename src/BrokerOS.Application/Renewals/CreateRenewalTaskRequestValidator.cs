using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class CreateRenewalTaskRequestValidator : AbstractValidator<CreateRenewalTaskRequest>
{
    public CreateRenewalTaskRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(2000);
        RuleFor(request => request.DueDateUtc).NotEmpty();
        RuleFor(request => request.Priority).IsInEnum();
    }
}
