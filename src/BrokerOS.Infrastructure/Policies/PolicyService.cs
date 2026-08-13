using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Policies;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Domain.Policies;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
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
        var policies = AccessiblePolicies().AsNoTracking();

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

        if (query.PolicyType.HasValue)
        {
            policies = policies.Where(policy => policy.PolicyType == query.PolicyType.Value);
        }

        if (query.ClientPublicId.HasValue)
        {
            policies = policies.Where(policy => policy.Client.PublicId == query.ClientPublicId.Value);
        }

        if (query.InsurerPublicId.HasValue)
        {
            policies = policies.Where(policy => policy.Insurer.PublicId == query.InsurerPublicId.Value);
        }

        if (query.AssignedUserPublicId.HasValue)
        {
            policies = policies.Where(policy =>
                policy.AssignedUser != null
                && policy.AssignedUser.PublicId == query.AssignedUserPublicId.Value);
        }

        if (query.FromDate.HasValue)
        {
            policies = policies.Where(policy => policy.ExpiryDate >= query.FromDate.Value);
        }

        if (query.ToDate.HasValue)
        {
            policies = policies.Where(policy => policy.ExpiryDate <= query.ToDate.Value);
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
            Items = entities.Select(policy => MapList(policy, today)).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<PolicyDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var policy = await GetAccessiblePolicyAsync(publicId, cancellationToken, asNoTracking: true);
        var activities = await LoadActivitiesAsync(policy.Id, cancellationToken);
        return MapDetails(policy, activities, _clock.Today);
    }

    public async Task<PolicyDetailsDto> CreateAsync(CreatePolicyRequest request, CancellationToken cancellationToken)
    {
        var client = await ResolveClientAsync(request.ClientPublicId, cancellationToken);
        var insurer = await ResolveInsurerAsync(request.InsurerPublicId, cancellationToken);
        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserPublicId, cancellationToken);
        var policyNumber = string.IsNullOrWhiteSpace(request.PolicyNumber)
            ? await AllocatePolicyNumberAsync(cancellationToken)
            : request.PolicyNumber.Trim();
        await EnsurePolicyNumberIsUniqueAsync(policyNumber, excludePolicyId: null, cancellationToken);

        var policy = new Policy
        {
            OrganizationId = _currentUser.OrganizationId,
            ClientId = client.Id,
            InsurerId = insurer.Id,
            PolicyNumber = policyNumber,
            PolicyType = request.PolicyType,
            StartDate = request.StartDate,
            ExpiryDate = request.ExpiryDate,
            Premium = request.Premium,
            SumInsured = request.SumInsured,
            CommissionPercentage = request.CommissionPercentage,
            CommissionAmount = CommissionCalculator.Amount(request.Premium, request.CommissionPercentage),
            AssignedUserId = assignedUser?.Id,
            Status = PolicyStatus.Active,
            Notes = TrimToNull(request.Notes),
            Client = client,
            Insurer = insurer,
            AssignedUser = assignedUser
        };

        _dbContext.Policies.Add(policy);
        AddActivity(policy, client.Id, ActivityType.PolicyCreated, $"Policy {policyNumber} created.");
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByPublicIdAsync(policy.PublicId, cancellationToken);
    }

    public async Task<PolicyDetailsDto> UpdateAsync(
        Guid publicId,
        UpdatePolicyRequest request,
        CancellationToken cancellationToken)
    {
        var policy = await GetAccessiblePolicyAsync(publicId, cancellationToken, asNoTracking: false);
        var client = await ResolveClientAsync(request.ClientPublicId, cancellationToken);
        var insurer = await ResolveInsurerAsync(request.InsurerPublicId, cancellationToken);
        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserPublicId, cancellationToken);
        var policyNumber = request.PolicyNumber.Trim();
        await EnsurePolicyNumberIsUniqueAsync(policyNumber, policy.Id, cancellationToken);

        policy.ClientId = client.Id;
        policy.InsurerId = insurer.Id;
        policy.PolicyNumber = policyNumber;
        policy.PolicyType = request.PolicyType;
        policy.StartDate = request.StartDate;
        policy.ExpiryDate = request.ExpiryDate;
        policy.Premium = request.Premium;
        policy.SumInsured = request.SumInsured;
        policy.CommissionPercentage = request.CommissionPercentage;
        policy.CommissionAmount = CommissionCalculator.Amount(request.Premium, request.CommissionPercentage);
        policy.AssignedUserId = assignedUser?.Id;
        policy.Notes = TrimToNull(request.Notes);
        policy.Client = client;
        policy.Insurer = insurer;
        policy.AssignedUser = assignedUser;

        AddActivity(policy, client.Id, ActivityType.PolicyUpdated, $"Policy {policyNumber} updated.");
        await _dbContext.SaveChangesAsync(cancellationToken);

        return await GetByPublicIdAsync(policy.PublicId, cancellationToken);
    }

    private IQueryable<Policy> AccessiblePolicies()
    {
        return _dbContext.Policies
            .Include(policy => policy.Client)
            .Include(policy => policy.Insurer)
            .Include(policy => policy.AssignedUser)
            .Include(policy => policy.PreviousPolicy)
            .Include(policy => policy.NextPolicy)
            .Include(policy => policy.Renewals)
            .ForCurrentUser(_currentUser);
    }

    private async Task<Policy> GetAccessiblePolicyAsync(Guid publicId, CancellationToken cancellationToken, bool asNoTracking)
    {
        var query = AccessiblePolicies().Where(policy => policy.PublicId == publicId);
        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var policy = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(policy);
        return policy!;
    }

    private async Task<IReadOnlyList<PolicyActivityDto>> LoadActivitiesAsync(long policyId, CancellationToken cancellationToken)
    {
        return await _dbContext.Activities
            .AsNoTracking()
            .Include(activity => activity.User)
            .Where(activity => activity.PolicyId == policyId)
            .OrderByDescending(activity => activity.CreatedAtUtc)
            .Select(activity => new PolicyActivityDto
            {
                PublicId = activity.PublicId,
                ActivityType = activity.ActivityType.ToString(),
                Description = activity.Description,
                CreatedAtUtc = activity.CreatedAtUtc,
                UserName = activity.User.FullName
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<Client> ResolveClientAsync(Guid clientPublicId, CancellationToken cancellationToken)
    {
        var client = await _dbContext.Clients
            .ForCurrentUser(_currentUser)
            .SingleOrDefaultAsync(entity => entity.PublicId == clientPublicId, cancellationToken);

        if (client is null)
        {
            throw new ValidationException([
                new ValidationFailure("ClientPublicId", "Client was not found.")
            ]);
        }

        return client;
    }

    private async Task<Insurer> ResolveInsurerAsync(Guid insurerPublicId, CancellationToken cancellationToken)
    {
        var insurer = await _dbContext.Insurers
            .SingleOrDefaultAsync(entity => entity.PublicId == insurerPublicId && entity.IsActive, cancellationToken);

        if (insurer is null)
        {
            throw new ValidationException([
                new ValidationFailure("InsurerPublicId", "Insurer was not found.")
            ]);
        }

        return insurer;
    }

    private async Task<User?> ResolveAssignedUserAsync(Guid? assignedUserPublicId, CancellationToken cancellationToken)
    {
        if (!assignedUserPublicId.HasValue)
        {
            return null;
        }

        var user = await _dbContext.Users
            .SingleOrDefaultAsync(entity => entity.PublicId == assignedUserPublicId.Value && entity.IsActive, cancellationToken);

        if (user is null)
        {
            throw new ValidationException([
                new ValidationFailure("AssignedUserPublicId", "Assigned user was not found.")
            ]);
        }

        return user;
    }

    private async Task<string> AllocatePolicyNumberAsync(CancellationToken cancellationToken)
    {
        var existing = await _dbContext.Policies
            .Select(policy => policy.PolicyNumber)
            .ToListAsync(cancellationToken);
        return PolicyNumberAllocator.Next(existing);
    }

    private async Task EnsurePolicyNumberIsUniqueAsync(string policyNumber, long? excludePolicyId, CancellationToken cancellationToken)
    {
        var exists = await _dbContext.Policies.AnyAsync(
            policy => policy.PolicyNumber == policyNumber
                && (!excludePolicyId.HasValue || policy.Id != excludePolicyId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException("A policy with this number already exists.");
        }
    }

    private void AddActivity(Policy policy, long clientId, ActivityType activityType, string description)
    {
        _dbContext.Activities.Add(new Activity
        {
            OrganizationId = policy.OrganizationId,
            ClientId = clientId,
            Policy = policy,
            UserId = _currentUser.UserId,
            ActivityType = activityType,
            Description = description
        });
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

    private static PolicyListDto MapList(Policy policy, DateOnly today) =>
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
            InsurerPublicId = policy.Insurer.PublicId,
            AssignedUserPublicId = policy.AssignedUser?.PublicId,
            AssignedUserName = policy.AssignedUser?.FullName,
            PreviousPolicyPublicId = policy.PreviousPolicy?.PublicId,
            NextPolicyPublicId = policy.NextPolicy?.PublicId
        };

    private static PolicyDetailsDto MapDetails(Policy policy, IReadOnlyList<PolicyActivityDto> activities, DateOnly today)
    {
        var renewal = policy.Renewals
            .Where(item => RenewalFactory.IsOpen(item.Status))
            .OrderBy(item => item.RenewalDate)
            .FirstOrDefault()
            ?? policy.Renewals.OrderByDescending(item => item.Id).FirstOrDefault();

        return new PolicyDetailsDto
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
            CommissionPercentage = policy.CommissionPercentage,
            CommissionAmount = policy.CommissionAmount,
            ClientPublicId = policy.Client.PublicId,
            ClientName = policy.Client.CompanyName,
            InsurerPublicId = policy.Insurer.PublicId,
            InsurerName = policy.Insurer.Name,
            AssignedUserPublicId = policy.AssignedUser?.PublicId,
            AssignedUserName = policy.AssignedUser?.FullName,
            RenewalPublicId = renewal?.PublicId,
            RenewalStatus = renewal?.Status.ToString(),
            RenewalPriority = renewal?.Priority.ToString(),
            RenewalStage = renewal?.CurrentStage.ToString(),
            Notes = policy.Notes,
            PreviousPolicyPublicId = policy.PreviousPolicy?.PublicId,
            NextPolicyPublicId = policy.NextPolicy?.PublicId,
            Activities = activities
        };
    }

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
