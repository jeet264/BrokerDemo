using FluentValidation;

namespace BrokerOS.Application.MyDay;

public sealed class MyDayActionRequestValidator : AbstractValidator<MyDayActionRequest>
{
    public MyDayActionRequestValidator()
    {
        RuleFor(request => request.Kind).IsInEnum();
        RuleFor(request => request.PublicId).NotEmpty();
    }
}
