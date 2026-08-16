using FluentValidation;

namespace BrokerOS.Application.Search;

public sealed class SearchQueryValidator : AbstractValidator<SearchQuery>
{
    public SearchQueryValidator()
    {
        RuleFor(query => query.Q).MaximumLength(100);
    }
}

public sealed class SearchQuery
{
    public string? Q { get; set; }
}
