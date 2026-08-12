using FluentValidation;

namespace BrokerOS.Application.Clients;

public sealed class UpdateClientRequestValidator : AbstractValidator<UpdateClientRequest>
{
    public UpdateClientRequestValidator()
    {
        RuleFor(request => request.ClientCode)
            .NotEmpty()
            .MaximumLength(50);

        RuleFor(request => request.CompanyName)
            .NotEmpty()
            .WithMessage("Company name is required")
            .MaximumLength(200);

        RuleFor(request => request.ClientType)
            .IsInEnum();

        RuleFor(request => request.Industry)
            .MaximumLength(100);

        RuleFor(request => request.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(request => request.Phone)
            .NotEmpty()
            .MaximumLength(30);

        RuleFor(request => request.AlternatePhone)
            .MaximumLength(30);

        RuleFor(request => request.AddressLine1)
            .NotEmpty()
            .MaximumLength(200);

        RuleFor(request => request.AddressLine2)
            .MaximumLength(200);

        RuleFor(request => request.City)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.State)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.PostalCode)
            .NotEmpty()
            .MaximumLength(20);

        RuleFor(request => request.Country)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(request => request.Notes)
            .MaximumLength(2000);
    }
}
