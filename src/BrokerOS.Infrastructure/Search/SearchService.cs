using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Search;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Search;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Search;

/// <summary>
/// Organisation-scoped desk search: client name/phone and policy number/vehicle number.
/// </summary>
/// <remarks>
/// Uses simple LIKE/Contains today. This method is the natural place to swap in SQL Server
/// full-text search, or an external search service, if performance or match quality becomes
/// an issue at scale. Callers (the controller and header UI) should not change.
/// </remarks>
public sealed class SearchService : ISearchService
{
    public const int MaxResults = 10;
    public const int MinQueryLength = 2;
    private const int CandidateCap = 40;

    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public SearchService(BrokerOsDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<SearchResultsDto> SearchAsync(string? query, CancellationToken cancellationToken)
    {
        var term = query?.Trim() ?? string.Empty;
        if (term.Length < MinQueryLength)
        {
            return new SearchResultsDto { Query = term, Items = [] };
        }

        var compact = SearchRelevance.Compact(term);

        // Contains/LIKE candidates. Rank in memory so exact matches sort ahead of partials.
        var clients = await _dbContext.Clients
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .Where(client =>
                client.CompanyName.Contains(term)
                || client.Phone.Contains(term)
                || (client.AlternatePhone != null && client.AlternatePhone.Contains(term)))
            .Select(client => new
            {
                client.PublicId,
                client.CompanyName,
                client.Phone,
                client.AlternatePhone,
                client.City
            })
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        var policies = await _dbContext.Policies
            .AsNoTracking()
            .ForCurrentUser(_currentUser)
            .Where(policy =>
                policy.PolicyNumber.Contains(term)
                || (policy.VehicleNumber != null && (
                    policy.VehicleNumber.Contains(term)
                    || (compact.Length >= 2
                        && policy.VehicleNumber.Replace("-", "").Replace(" ", "").Contains(compact)))))
            .Select(policy => new
            {
                policy.PublicId,
                policy.PolicyNumber,
                policy.VehicleNumber,
                ClientName = policy.Client.CompanyName
            })
            .Take(CandidateCap)
            .ToListAsync(cancellationToken);

        var hits = new List<(int Rank, string Title, SearchHitDto Hit)>(clients.Count + policies.Count);

        foreach (var client in clients)
        {
            var nameRank = SearchRelevance.Rank(client.CompanyName, term);
            var phoneRank = SearchRelevance.Best(term, client.Phone, client.AlternatePhone);
            var rank = Math.Min(nameRank, phoneRank);
            if (rank == SearchRelevance.None)
            {
                continue;
            }

            hits.Add((
                rank,
                client.CompanyName,
                new SearchHitDto
                {
                    Type = "Client",
                    PublicId = client.PublicId,
                    Title = client.CompanyName,
                    Subtitle = string.IsNullOrWhiteSpace(client.City)
                        ? client.Phone
                        : $"{client.Phone} · {client.City}",
                    MatchedOn = nameRank <= phoneRank ? "Name" : "Phone"
                }));
        }

        foreach (var policy in policies)
        {
            var numberRank = SearchRelevance.Rank(policy.PolicyNumber, term);
            var vehicleRank = SearchRelevance.Rank(policy.VehicleNumber, term);
            var rank = Math.Min(numberRank, vehicleRank);
            if (rank == SearchRelevance.None)
            {
                continue;
            }

            var subtitle = string.IsNullOrWhiteSpace(policy.VehicleNumber)
                ? policy.ClientName
                : $"{policy.ClientName} · {policy.VehicleNumber}";

            hits.Add((
                rank,
                policy.PolicyNumber,
                new SearchHitDto
                {
                    Type = "Policy",
                    PublicId = policy.PublicId,
                    Title = policy.PolicyNumber,
                    Subtitle = subtitle,
                    MatchedOn = numberRank <= vehicleRank ? "PolicyNumber" : "VehicleNumber"
                }));
        }

        var items = hits
            .OrderBy(item => item.Rank)
            .ThenBy(item => item.Title, StringComparer.OrdinalIgnoreCase)
            .Take(MaxResults)
            .Select(item => item.Hit)
            .ToList();

        return new SearchResultsDto { Query = term, Items = items };
    }
}
