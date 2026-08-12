using FluentValidation;

namespace BrokerOS.Application.Insurers;

public sealed class UpdateInsurerRequestValidator : AbstractValidator<UpdateInsurerRequest>
{
    public UpdateInsurerRequestValidator()
    {
        Include(new InsurerWriteRequestValidator<UpdateInsurerRequest>());
    }
}
