using FluentValidation;

namespace BrokerOS.Application.Insurers;

public sealed class InsurerListQueryValidator : AbstractValidator<InsurerListQuery>
{
    public InsurerListQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.SortBy).MaximumLength(50);
        RuleFor(query => query.SortDir).MaximumLength(10);
    }
}
