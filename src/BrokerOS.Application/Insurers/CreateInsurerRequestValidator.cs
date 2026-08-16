using FluentValidation;

namespace BrokerOS.Application.Insurers;

public sealed class CreateInsurerRequestValidator : AbstractValidator<CreateInsurerRequest>
{
    public CreateInsurerRequestValidator()
    {
        Include(new InsurerWriteRequestValidator<CreateInsurerRequest>());
    }
}
