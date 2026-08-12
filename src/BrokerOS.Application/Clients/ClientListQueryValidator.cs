using FluentValidation;

namespace BrokerOS.Application.Clients;

public sealed class ClientListQueryValidator : AbstractValidator<ClientListQuery>
{
    public ClientListQueryValidator()
    {
        RuleFor(query => query.Page).GreaterThanOrEqualTo(1);
        RuleFor(query => query.PageSize).InclusiveBetween(1, 100);
        RuleFor(query => query.Search).MaximumLength(200);
        RuleFor(query => query.Industry).MaximumLength(100);
        RuleFor(query => query.SortBy).MaximumLength(50);
        RuleFor(query => query.SortDir).MaximumLength(10);
    }
}
