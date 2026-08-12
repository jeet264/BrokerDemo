using FluentValidation;

namespace BrokerOS.Application.Auth;

public sealed class RegisterOrganizationRequestValidator : AbstractValidator<RegisterOrganizationRequest>
{
    public RegisterOrganizationRequestValidator()
    {
        RuleFor(request => request.OrganizationName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.OrganizationCode)
            .NotEmpty()
            .MinimumLength(2)
            .MaximumLength(50)
            .Matches("^[A-Za-z0-9_-]+$")
            .WithMessage("Organization code may contain letters, numbers, hyphens, and underscores only.");

        RuleFor(request => request.AdminFullName)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.AdminEmail)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.AdminPassword)
            .NotEmpty()
            .MinimumLength(8)
            .MaximumLength(200)
            .Matches("[A-Za-z]").WithMessage("Password must contain at least one letter.")
            .Matches("[0-9]").WithMessage("Password must contain at least one number.");
    }
}
