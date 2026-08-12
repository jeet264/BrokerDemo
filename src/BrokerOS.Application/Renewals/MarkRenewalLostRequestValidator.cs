using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class MarkRenewalLostRequestValidator : AbstractValidator<MarkRenewalLostRequest>
{
    public MarkRenewalLostRequestValidator()
    {
        RuleFor(request => request.Reason).MaximumLength(2000);
    }
}
