using FluentValidation;

namespace BrokerOS.Application.Quotations;

public sealed class UpdateQuotationRequestValidator : AbstractValidator<UpdateQuotationRequest>
{
    public UpdateQuotationRequestValidator()
    {
        RuleFor(request => request)
            .Must(HasExactlyOneInsurerSource)
            .WithMessage("Provide either an existing insurer or a new insurer name.");

        RuleFor(request => request.NewInsurerName)
            .MaximumLength(200)
            .When(request => !string.IsNullOrWhiteSpace(request.NewInsurerName));

        RuleFor(request => request.PremiumAmount).GreaterThanOrEqualTo(0);
        RuleFor(request => request.SumInsured).GreaterThanOrEqualTo(0).When(request => request.SumInsured.HasValue);
        RuleFor(request => request.CoverageSummary).MaximumLength(1000);
        RuleFor(request => request.Notes).MaximumLength(2000);
    }

    private static bool HasExactlyOneInsurerSource(UpdateQuotationRequest request)
    {
        var hasExisting = request.InsurerPublicId.HasValue && request.InsurerPublicId.Value != Guid.Empty;
        var hasNewName = !string.IsNullOrWhiteSpace(request.NewInsurerName);
        return hasExisting ^ hasNewName;
    }
}
