using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Clients;
using BrokerOS.Application.Common;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Clients;

public sealed class ClientService : IClientService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public ClientService(BrokerOsDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<ClientListDto>> ListAsync(ClientListQuery query, CancellationToken cancellationToken)
    {
        var clients = _dbContext.Clients
            .AsNoTracking()
            .Include(client => client.AssignedUser)
            .ForCurrentUser(_currentUser)
            .AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            clients = clients.Where(client =>
                client.CompanyName.Contains(term)
                || client.ClientCode.Contains(term)
                || client.Email.Contains(term)
                || client.Phone.Contains(term));
        }

        if (query.ClientType.HasValue)
        {
            clients = clients.Where(client => client.ClientType == query.ClientType.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Industry))
        {
            var industry = query.Industry.Trim();
            clients = clients.Where(client => client.Industry != null && client.Industry.Contains(industry));
        }

        if (query.AssignedUserPublicId.HasValue)
        {
            clients = clients.Where(client =>
                client.AssignedUser != null
                && client.AssignedUser.PublicId == query.AssignedUserPublicId.Value);
        }

        if (query.IsActive.HasValue)
        {
            clients = clients.Where(client => client.IsActive == query.IsActive.Value);
        }

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        clients = ApplySort(clients, query.SortBy, descending);

        var totalCount = await clients.CountAsync(cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var entities = await clients
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<ClientListDto>
        {
            Items = entities.Select(MapList).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<ClientDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var client = await GetAccessibleClientAsync(publicId, cancellationToken, asNoTracking: true);
        return MapDetails(client);
    }

    public async Task<ClientDetailsDto> CreateAsync(CreateClientRequest request, CancellationToken cancellationToken)
    {
        await EnsureClientCodeIsUniqueAsync(request.ClientCode, excludeClientId: null, cancellationToken);
        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserPublicId, cancellationToken);

        var client = new Client
        {
            OrganizationId = _currentUser.OrganizationId,
            ClientCode = request.ClientCode.Trim(),
            CompanyName = request.CompanyName.Trim(),
            ClientType = request.ClientType,
            Industry = TrimToNull(request.Industry),
            Email = request.Email.Trim().ToLowerInvariant(),
            Phone = request.Phone.Trim(),
            AlternatePhone = TrimToNull(request.AlternatePhone),
            AddressLine1 = request.AddressLine1.Trim(),
            AddressLine2 = TrimToNull(request.AddressLine2),
            City = request.City.Trim(),
            State = request.State.Trim(),
            PostalCode = request.PostalCode.Trim(),
            Country = string.IsNullOrWhiteSpace(request.Country) ? "India" : request.Country.Trim(),
            AssignedUserId = assignedUser?.Id,
            Notes = TrimToNull(request.Notes),
            IsActive = request.IsActive
        };

        _dbContext.Clients.Add(client);
        await _dbContext.SaveChangesAsync(cancellationToken);

        client.AssignedUser = assignedUser;
        return MapDetails(client);
    }

    public async Task<ClientDetailsDto> UpdateAsync(Guid publicId, UpdateClientRequest request, CancellationToken cancellationToken)
    {
        var client = await GetAccessibleClientAsync(publicId, cancellationToken, asNoTracking: false);
        await EnsureClientCodeIsUniqueAsync(request.ClientCode, client.Id, cancellationToken);
        var assignedUser = await ResolveAssignedUserAsync(request.AssignedUserPublicId, cancellationToken);

        client.ClientCode = request.ClientCode.Trim();
        client.CompanyName = request.CompanyName.Trim();
        client.ClientType = request.ClientType;
        client.Industry = TrimToNull(request.Industry);
        client.Email = request.Email.Trim().ToLowerInvariant();
        client.Phone = request.Phone.Trim();
        client.AlternatePhone = TrimToNull(request.AlternatePhone);
        client.AddressLine1 = request.AddressLine1.Trim();
        client.AddressLine2 = TrimToNull(request.AddressLine2);
        client.City = request.City.Trim();
        client.State = request.State.Trim();
        client.PostalCode = request.PostalCode.Trim();
        client.Country = string.IsNullOrWhiteSpace(request.Country) ? "India" : request.Country.Trim();
        client.AssignedUserId = assignedUser?.Id;
        client.Notes = TrimToNull(request.Notes);
        client.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync(cancellationToken);

        client.AssignedUser = assignedUser;
        return MapDetails(client);
    }

    public async Task DeleteAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var client = await GetAccessibleClientAsync(publicId, cancellationToken, asNoTracking: false);
        _dbContext.Clients.Remove(client);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClientPolicyDto>> ListPoliciesAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var client = await GetAccessibleClientAsync(publicId, cancellationToken, asNoTracking: true);

        return await _dbContext.Policies
            .AsNoTracking()
            .Include(policy => policy.Insurer)
            .Include(policy => policy.AssignedUser)
            .Include(policy => policy.PreviousPolicy)
            .Include(policy => policy.NextPolicy)
            .Where(policy => policy.ClientId == client.Id)
            .OrderByDescending(policy => policy.Status == PolicyStatus.Active)
            .ThenByDescending(policy => policy.ExpiryDate)
            .Select(policy => new ClientPolicyDto
            {
                PublicId = policy.PublicId,
                PolicyNumber = policy.PolicyNumber,
                PolicyType = policy.PolicyType.ToString(),
                Status = policy.Status.ToString(),
                StartDate = policy.StartDate,
                ExpiryDate = policy.ExpiryDate,
                Premium = policy.Premium,
                SumInsured = policy.SumInsured,
                InsurerName = policy.Insurer.Name,
                AssignedUserPublicId = policy.AssignedUser != null ? policy.AssignedUser.PublicId : null,
                PreviousPolicyPublicId = policy.PreviousPolicy != null ? policy.PreviousPolicy.PublicId : null,
                NextPolicyPublicId = policy.NextPolicy != null ? policy.NextPolicy.PublicId : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClientRenewalDto>> ListRenewalsAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var client = await GetAccessibleClientAsync(publicId, cancellationToken, asNoTracking: true);

        return await _dbContext.Renewals
            .AsNoTracking()
            .Include(renewal => renewal.Policy)
            .Include(renewal => renewal.AssignedUser)
            .Where(renewal => renewal.Policy.ClientId == client.Id)
            .OrderBy(renewal => renewal.RenewalDate)
            .Select(renewal => new ClientRenewalDto
            {
                PublicId = renewal.PublicId,
                PolicyPublicId = renewal.Policy.PublicId,
                PolicyNumber = renewal.Policy.PolicyNumber,
                RenewalDate = renewal.RenewalDate,
                Status = renewal.Status.ToString(),
                Priority = renewal.Priority.ToString(),
                CurrentStage = renewal.CurrentStage.ToString(),
                AssignedUserPublicId = renewal.AssignedUser != null ? renewal.AssignedUser.PublicId : null
            })
            .ToListAsync(cancellationToken);
    }

    public async Task<IReadOnlyList<ClientActivityDto>> ListActivitiesAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var client = await GetAccessibleClientAsync(publicId, cancellationToken, asNoTracking: true);

        return await _dbContext.Activities
            .AsNoTracking()
            .Include(activity => activity.User)
            .Where(activity => activity.ClientId == client.Id)
            .OrderByDescending(activity => activity.CreatedAtUtc)
            .Select(activity => new ClientActivityDto
            {
                PublicId = activity.PublicId,
                ActivityType = activity.ActivityType.ToString(),
                Description = activity.Description,
                CreatedAtUtc = activity.CreatedAtUtc,
                UserName = activity.User.FullName
            })
            .ToListAsync(cancellationToken);
    }

    private async Task<Client> GetAccessibleClientAsync(Guid publicId, CancellationToken cancellationToken, bool asNoTracking)
    {
        var query = _dbContext.Clients
            .Include(client => client.AssignedUser)
            .ForCurrentUser(_currentUser)
            .Where(client => client.PublicId == publicId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var client = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(client);
        return client!;
    }

    private async Task EnsureClientCodeIsUniqueAsync(string clientCode, long? excludeClientId, CancellationToken cancellationToken)
    {
        var code = clientCode.Trim();
        var exists = await _dbContext.Clients.AnyAsync(
            client => client.ClientCode == code && (!excludeClientId.HasValue || client.Id != excludeClientId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException("A client with this code already exists.");
        }
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

    private static IQueryable<Client> ApplySort(IQueryable<Client> query, string? sortBy, bool descending)
    {
        return (sortBy?.Trim().ToLowerInvariant(), descending) switch
        {
            ("clientcode", false) => query.OrderBy(client => client.ClientCode),
            ("clientcode", true) => query.OrderByDescending(client => client.ClientCode),
            ("email", false) => query.OrderBy(client => client.Email),
            ("email", true) => query.OrderByDescending(client => client.Email),
            ("phone", false) => query.OrderBy(client => client.Phone),
            ("phone", true) => query.OrderByDescending(client => client.Phone),
            ("industry", false) => query.OrderBy(client => client.Industry),
            ("industry", true) => query.OrderByDescending(client => client.Industry),
            ("clienttype", false) => query.OrderBy(client => client.ClientType),
            ("clienttype", true) => query.OrderByDescending(client => client.ClientType),
            ("isactive", false) => query.OrderBy(client => client.IsActive),
            ("isactive", true) => query.OrderByDescending(client => client.IsActive),
            ("createdatutc", false) => query.OrderBy(client => client.CreatedAtUtc),
            ("createdatutc", true) => query.OrderByDescending(client => client.CreatedAtUtc),
            ("companyname", true) => query.OrderByDescending(client => client.CompanyName),
            _ => query.OrderBy(client => client.CompanyName)
        };
    }

    private static ClientListDto MapList(Client client) =>
        new()
        {
            PublicId = client.PublicId,
            ClientCode = client.ClientCode,
            CompanyName = client.CompanyName,
            ClientType = client.ClientType.ToString(),
            Industry = client.Industry,
            Email = client.Email,
            Phone = client.Phone,
            City = client.City,
            State = client.State,
            IsActive = client.IsActive,
            AssignedUserPublicId = client.AssignedUser?.PublicId,
            AssignedUserName = client.AssignedUser?.FullName
        };

    private static ClientDetailsDto MapDetails(Client client) =>
        new()
        {
            PublicId = client.PublicId,
            ClientCode = client.ClientCode,
            CompanyName = client.CompanyName,
            ClientType = client.ClientType.ToString(),
            Industry = client.Industry,
            Email = client.Email,
            Phone = client.Phone,
            AlternatePhone = client.AlternatePhone,
            AddressLine1 = client.AddressLine1,
            AddressLine2 = client.AddressLine2,
            City = client.City,
            State = client.State,
            PostalCode = client.PostalCode,
            Country = client.Country,
            AssignedUserPublicId = client.AssignedUser?.PublicId,
            AssignedUserName = client.AssignedUser?.FullName,
            Notes = client.Notes,
            IsActive = client.IsActive,
            CreatedAtUtc = client.CreatedAtUtc,
            ModifiedAtUtc = client.ModifiedAtUtc
        };

    private static string? TrimToNull(string? value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return null;
        }

        return value.Trim();
    }
}
