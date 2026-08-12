using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class CompleteRenewalRequestValidator : AbstractValidator<CompleteRenewalRequest>
{
    public CompleteRenewalRequestValidator()
    {
        RuleFor(request => request.Notes).MaximumLength(2000);
        RuleFor(request => request.Premium).GreaterThanOrEqualTo(0).When(request => request.Premium.HasValue);
        RuleFor(request => request.SumInsured).GreaterThanOrEqualTo(0).When(request => request.SumInsured.HasValue);
        RuleFor(request => request.CommissionPercentage)
            .InclusiveBetween(0, 100)
            .When(request => request.CommissionPercentage.HasValue);
    }
}
