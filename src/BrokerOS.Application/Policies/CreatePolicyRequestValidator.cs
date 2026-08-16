using FluentValidation;

namespace BrokerOS.Application.Policies;

public sealed class CreatePolicyRequestValidator : AbstractValidator<CreatePolicyRequest>
{
    public CreatePolicyRequestValidator()
    {
        RuleFor(request => request.PolicyNumber)
            .MaximumLength(50)
            .When(request => !string.IsNullOrWhiteSpace(request.PolicyNumber));

        RuleFor(request => request.ClientPublicId).NotEmpty();
        RuleFor(request => request.InsurerPublicId).NotEmpty();
        RuleFor(request => request.PolicyType).IsInEnum();
        RuleFor(request => request.ExpiryDate)
            .GreaterThan(request => request.StartDate)
            .WithMessage("Expiry date must be after the start date.");
        RuleFor(request => request.Premium).GreaterThanOrEqualTo(0);
        RuleFor(request => request.SumInsured).GreaterThanOrEqualTo(0);
        RuleFor(request => request.CommissionPercentage).InclusiveBetween(0, 100);
        RuleFor(request => request.VehicleNumber).MaximumLength(20);
        RuleFor(request => request.Notes).MaximumLength(2000);
    }
}
