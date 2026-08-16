using BrokerOS.Application.Search;

namespace BrokerOS.Application.Abstractions;

public interface ISearchService
{
    Task<SearchResultsDto> SearchAsync(string? query, CancellationToken cancellationToken);
}
