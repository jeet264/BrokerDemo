using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class CompleteRenewalRequestValidator : AbstractValidator<CompleteRenewalRequest>
{
    public CompleteRenewalRequestValidator()
    {
        RuleFor(request => request.Notes).MaximumLength(2000);
    }
}
