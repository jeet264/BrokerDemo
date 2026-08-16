using FluentValidation;

namespace BrokerOS.Application.Import;

public sealed class ImportConfirmRequestValidator : AbstractValidator<ImportConfirmRequest>
{
    public ImportConfirmRequestValidator()
    {
        RuleFor(request => request.PreviewToken)
            .NotEmpty()
            .WithMessage("Preview token is required. Upload the file for preview first.");
    }
}
