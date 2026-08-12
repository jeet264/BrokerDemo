using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Common;
using BrokerOS.Application.Insurers;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Insurers;

public sealed class InsurerService : IInsurerService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;

    public InsurerService(BrokerOsDbContext dbContext, ICurrentUserService currentUser)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
    }

    public async Task<PagedResult<InsurerListDto>> ListAsync(InsurerListQuery query, CancellationToken cancellationToken)
    {
        var insurers = _dbContext.Insurers.AsNoTracking().AsQueryable();

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            var term = query.Search.Trim();
            insurers = insurers.Where(insurer =>
                insurer.Name.Contains(term)
                || insurer.Code.Contains(term)
                || (insurer.Email != null && insurer.Email.Contains(term))
                || (insurer.Phone != null && insurer.Phone.Contains(term)));
        }

        if (query.IsActive.HasValue)
        {
            insurers = insurers.Where(insurer => insurer.IsActive == query.IsActive.Value);
        }

        var descending = string.Equals(query.SortDir, "desc", StringComparison.OrdinalIgnoreCase);
        insurers = ApplySort(insurers, query.SortBy, descending);

        var totalCount = await insurers.CountAsync(cancellationToken);
        var page = query.Page < 1 ? 1 : query.Page;
        var pageSize = query.PageSize is < 1 or > 100 ? 20 : query.PageSize;

        var entities = await insurers
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<InsurerListDto>
        {
            Items = entities.Select(MapList).ToList(),
            Page = page,
            PageSize = pageSize,
            TotalCount = totalCount
        };
    }

    public async Task<InsurerDetailsDto> GetByPublicIdAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var insurer = await GetAccessibleInsurerAsync(publicId, cancellationToken, asNoTracking: true);
        return MapDetails(insurer);
    }

    public async Task<InsurerDetailsDto> CreateAsync(CreateInsurerRequest request, CancellationToken cancellationToken)
    {
        await EnsureNameIsUniqueAsync(request.Name, excludeInsurerId: null, cancellationToken);
        await EnsureCodeIsUniqueAsync(request.Code, excludeInsurerId: null, cancellationToken);

        var insurer = new Insurer
        {
            OrganizationId = _currentUser.OrganizationId,
            Name = request.Name.Trim(),
            Code = request.Code.Trim(),
            Email = TrimToNull(request.Email)?.ToLowerInvariant(),
            Phone = TrimToNull(request.Phone),
            Website = TrimToNull(request.Website),
            IsActive = request.IsActive
        };

        _dbContext.Insurers.Add(insurer);
        await SaveChangesHandlingDuplicatesAsync(cancellationToken);
        return MapDetails(insurer);
    }

    public async Task<InsurerDetailsDto> UpdateAsync(Guid publicId, UpdateInsurerRequest request, CancellationToken cancellationToken)
    {
        var insurer = await GetAccessibleInsurerAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureTenantOwned(insurer);

        await EnsureNameIsUniqueAsync(request.Name, insurer.Id, cancellationToken);
        await EnsureCodeIsUniqueAsync(request.Code, insurer.Id, cancellationToken);

        insurer.Name = request.Name.Trim();
        insurer.Code = request.Code.Trim();
        insurer.Email = TrimToNull(request.Email)?.ToLowerInvariant();
        insurer.Phone = TrimToNull(request.Phone);
        insurer.Website = TrimToNull(request.Website);
        insurer.IsActive = request.IsActive;

        await SaveChangesHandlingDuplicatesAsync(cancellationToken);
        return MapDetails(insurer);
    }

    public async Task DeleteAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var insurer = await GetAccessibleInsurerAsync(publicId, cancellationToken, asNoTracking: false);
        EnsureTenantOwned(insurer);

        var hasPolicies = await _dbContext.Policies
            .IgnoreQueryFilters()
            .AnyAsync(policy => policy.InsurerId == insurer.Id, cancellationToken);

        if (hasPolicies)
        {
            throw new ConflictException("This insurer cannot be deleted because policies are linked to it.");
        }

        _dbContext.Insurers.Remove(insurer);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    private async Task<Insurer> GetAccessibleInsurerAsync(Guid publicId, CancellationToken cancellationToken, bool asNoTracking)
    {
        var query = _dbContext.Insurers.Where(insurer => insurer.PublicId == publicId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var insurer = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(insurer);
        return insurer!;
    }

    private static void EnsureTenantOwned(Insurer insurer)
    {
        if (insurer.OrganizationId is null)
        {
            throw new ForbiddenException("System insurers cannot be modified.");
        }
    }

    private async Task EnsureNameIsUniqueAsync(string name, long? excludeInsurerId, CancellationToken cancellationToken)
    {
        var trimmed = name.Trim();
        var exists = await _dbContext.Insurers.AnyAsync(
            insurer => insurer.Name == trimmed && (!excludeInsurerId.HasValue || insurer.Id != excludeInsurerId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException("An insurer with this name already exists.");
        }
    }

    private async Task EnsureCodeIsUniqueAsync(string code, long? excludeInsurerId, CancellationToken cancellationToken)
    {
        var trimmed = code.Trim();
        var exists = await _dbContext.Insurers.AnyAsync(
            insurer => insurer.Code == trimmed && (!excludeInsurerId.HasValue || insurer.Id != excludeInsurerId.Value),
            cancellationToken);

        if (exists)
        {
            throw new ConflictException("An insurer with this code already exists.");
        }
    }

    private async Task SaveChangesHandlingDuplicatesAsync(CancellationToken cancellationToken)
    {
        try
        {
            await _dbContext.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateException exception) when (IsUniqueViolation(exception))
        {
            throw new ConflictException("An insurer with this name or code already exists.");
        }
    }

    private static bool IsUniqueViolation(DbUpdateException exception) =>
        exception.InnerException is SqlException sql && (sql.Number is 2601 or 2627);

    private static IQueryable<Insurer> ApplySort(IQueryable<Insurer> query, string? sortBy, bool descending)
    {
        return (sortBy?.Trim().ToLowerInvariant(), descending) switch
        {
            ("code", false) => query.OrderBy(insurer => insurer.Code),
            ("code", true) => query.OrderByDescending(insurer => insurer.Code),
            ("email", false) => query.OrderBy(insurer => insurer.Email),
            ("email", true) => query.OrderByDescending(insurer => insurer.Email),
            ("phone", false) => query.OrderBy(insurer => insurer.Phone),
            ("phone", true) => query.OrderByDescending(insurer => insurer.Phone),
            ("isactive", false) => query.OrderBy(insurer => insurer.IsActive),
            ("isactive", true) => query.OrderByDescending(insurer => insurer.IsActive),
            ("isglobal", false) => query.OrderBy(insurer => insurer.OrganizationId == null),
            ("isglobal", true) => query.OrderByDescending(insurer => insurer.OrganizationId == null),
            ("createdatutc", false) => query.OrderBy(insurer => insurer.CreatedAtUtc),
            ("createdatutc", true) => query.OrderByDescending(insurer => insurer.CreatedAtUtc),
            ("name", true) => query.OrderByDescending(insurer => insurer.Name),
            _ => query.OrderBy(insurer => insurer.Name)
        };
    }

    private static InsurerListDto MapList(Insurer insurer) =>
        new()
        {
            PublicId = insurer.PublicId,
            Name = insurer.Name,
            Code = insurer.Code,
            Email = insurer.Email,
            Phone = insurer.Phone,
            Website = insurer.Website,
            IsActive = insurer.IsActive,
            IsGlobal = insurer.OrganizationId is null
        };

    private static InsurerDetailsDto MapDetails(Insurer insurer) =>
        new()
        {
            PublicId = insurer.PublicId,
            Name = insurer.Name,
            Code = insurer.Code,
            Email = insurer.Email,
            Phone = insurer.Phone,
            Website = insurer.Website,
            IsActive = insurer.IsActive,
            IsGlobal = insurer.OrganizationId is null,
            CreatedAtUtc = insurer.CreatedAtUtc,
            ModifiedAtUtc = insurer.ModifiedAtUtc
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
