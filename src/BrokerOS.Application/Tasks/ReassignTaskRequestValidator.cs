using FluentValidation;

namespace BrokerOS.Application.Tasks;

public sealed class ReassignTaskRequestValidator : AbstractValidator<ReassignTaskRequest>
{
    public ReassignTaskRequestValidator()
    {
        RuleFor(request => request.AssignedUserPublicId).NotEmpty();
    }
}
