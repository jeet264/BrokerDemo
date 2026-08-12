using FluentValidation;

namespace BrokerOS.Application.Insurers;

internal sealed class InsurerWriteRequestValidator<T> : AbstractValidator<T>
    where T : IInsurerWriteRequest
{
    public InsurerWriteRequestValidator()
    {
        RuleFor(request => request.Name)
            .NotEmpty()
            .WithMessage("Name is required")
            .MaximumLength(200);

        RuleFor(request => request.Code)
            .NotEmpty()
            .WithMessage("Code is required")
            .MaximumLength(50);

        RuleFor(request => request.Email)
            .MaximumLength(256)
            .EmailAddress()
            .When(request => !string.IsNullOrWhiteSpace(request.Email));

        RuleFor(request => request.Phone)
            .MaximumLength(30);

        RuleFor(request => request.Website)
            .MaximumLength(300)
            .Must(BeAValidWebsite)
            .When(request => !string.IsNullOrWhiteSpace(request.Website))
            .WithMessage("Website must be a valid URL.");
    }

    private static bool BeAValidWebsite(string? website)
    {
        if (string.IsNullOrWhiteSpace(website))
        {
            return true;
        }

        return Uri.TryCreate(website.Trim(), UriKind.Absolute, out var uri)
            && (uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps);
    }
}
