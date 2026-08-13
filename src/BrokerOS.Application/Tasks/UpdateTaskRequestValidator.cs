using FluentValidation;

namespace BrokerOS.Application.Tasks;

public sealed class UpdateTaskRequestValidator : AbstractValidator<UpdateTaskRequest>
{
    public UpdateTaskRequestValidator()
    {
        RuleFor(request => request.Title).NotEmpty().MaximumLength(200);
        RuleFor(request => request.Description).MaximumLength(2000);
        RuleFor(request => request.DueDateUtc).NotEmpty();
        RuleFor(request => request.Priority).IsInEnum();
    }
}
