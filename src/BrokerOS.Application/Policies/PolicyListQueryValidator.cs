using FluentValidation;

namespace BrokerOS.Application.Policies;

public sealed class PolicyListQueryValidator : AbstractValidator<PolicyListQuery>
{
    public PolicyListQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.SortBy).MaximumLength(50);
        RuleFor(query => query.SortDir).MaximumLength(10);
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
    }
}
