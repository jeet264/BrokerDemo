using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Policies;

public sealed class PolicyService : IPolicyService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly IClock _clock;

    public PolicyService(BrokerOsDbContext dbContext, ICurrentUserService currentUser, IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _clock = clock;
    }

    public async Task<PagedResult<PolicyListDto>> ListAsync(PolicyListQuery query, CancellationToken cancellationToken)
    {
        var today = _clock.Today;
        var policies = _dbContext.Policies
            .AsNoTracking()
            .Include(policy => policy.Client)
            .Include(policy => policy.Insurer)
            .Include(policy => policy.AssignedUser)
            .Include(policy => policy.PreviousPolicy)
            .Include(policy => policy.NextPolicy)
            .ForCurrentUser(_currentUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            policies = policies.Where(policy =>
                policy.PolicyNumber.Contains(term)
                || policy.Client.CompanyName.Contains(term)
                || policy.Insurer.Name.Contains(term));
        }

        if (query.Status.HasValue)
        {
            policies = policies.Where(policy => policy.Status == query.Status.Value);
        }

        if (query.ClientPublicId.HasValue)
        {
            policies = policies.Where(policy => policy.Client.PublicId == query.ClientPublicId.Value);
        }

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        policies = ApplySort(policies, query.SortBy, descending);

        var totalCount = await policies.CountAsync(cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var entities = await policies
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<PolicyListDto>
        {
            Items = entities.Select(policy => Map(policy, today)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    private static IQueryable<Policy> ApplySort(IQueryable<Policy> query, string? sortBy, bool descending)
    {
        return (sortBy?.Trim().ToLowerInvariant(), descending) switch
        {
            ("policynumber", false) => query.OrderBy(policy => policy.PolicyNumber),
            ("policynumber", true) => query.OrderByDescending(policy => policy.PolicyNumber),
            ("status", false) => query.OrderBy(policy => policy.Status),
            ("status", true) => query.OrderByDescending(policy => policy.Status),
            ("premium", false) => query.OrderBy(policy => policy.Premium),
            ("premium", true) => query.OrderByDescending(policy => policy.Premium),
            ("clientname", false) => query.OrderBy(policy => policy.Client.CompanyName),
            ("clientname", true) => query.OrderByDescending(policy => policy.Client.CompanyName),
            ("startdate", false) => query.OrderBy(policy => policy.StartDate),
            ("startdate", true) => query.OrderByDescending(policy => policy.StartDate),
            ("expirydate", true) => query.OrderByDescending(policy => policy.ExpiryDate),
            _ => query.OrderBy(policy => policy.ExpiryDate)
        };
    }

    private static PolicyListDto Map(Policy policy, DateOnly today) =>
        new()
        {
            PublicId = policy.PublicId,
            PolicyNumber = policy.PolicyNumber,
            PolicyType = policy.PolicyType.ToString(),
            Status = policy.Status.ToString(),
            StartDate = policy.StartDate,
            ExpiryDate = policy.ExpiryDate,
            DaysRemaining = policy.ExpiryDate.DayNumber - today.DayNumber,
            Premium = policy.Premium,
            SumInsured = policy.SumInsured,
            ClientName = policy.Client.CompanyName,
            ClientPublicId = policy.Client.PublicId,
            InsurerName = policy.Insurer.Name,
            AssignedUserPublicId = policy.AssignedUser?.PublicId,
            AssignedUserName = policy.AssignedUser?.FullName,
            PreviousPolicyPublicId = policy.PreviousPolicy?.PublicId,
            NextPolicyPublicId = policy.NextPolicy?.PublicId
        };
}
