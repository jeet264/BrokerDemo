using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class UpdateRenewalStageRequestValidator : AbstractValidator<UpdateRenewalStageRequest>
{
    public UpdateRenewalStageRequestValidator()
    {
        RuleFor(request => request.Stage).IsInEnum();
        RuleFor(request => request.Notes).MaximumLength(2000);
    }
}
