using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class UpdateRenewalStatusRequestValidator : AbstractValidator<UpdateRenewalStatusRequest>
{
    public UpdateRenewalStatusRequestValidator()
    {
        RuleFor(request => request.Status).IsInEnum();
        RuleFor(request => request.Notes).MaximumLength(2000);
    }
}
