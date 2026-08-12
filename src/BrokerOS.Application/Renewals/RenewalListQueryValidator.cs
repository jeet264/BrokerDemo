using FluentValidation;

namespace BrokerOS.Application.Renewals;

public sealed class RenewalListQueryValidator : AbstractValidator<RenewalListQuery>
{
    public RenewalListQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.SortBy).MaximumLength(50);
        RuleFor(query => query.SortDir).MaximumLength(10);
        RuleFor(query => query.DueWithinDays).InclusiveBetween(0, 365).When(query => query.DueWithinDays.HasValue);
        RuleFor(query => query.Status).IsInEnum().When(query => query.Status.HasValue);
        RuleFor(query => query.Stage).IsInEnum().When(query => query.Stage.HasValue);
        RuleFor(query => query.Priority).IsInEnum().When(query => query.Priority.HasValue);
    }
}
