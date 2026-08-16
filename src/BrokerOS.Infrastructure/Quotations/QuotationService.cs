using BrokerOS.Application.Abstractions;
using BrokerOS.Application.Notifications;
using BrokerOS.Application.Quotations;
using BrokerOS.Application.Security;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Enums;
using BrokerOS.Domain.Exceptions;
using BrokerOS.Domain.Quotations;
using BrokerOS.Domain.Renewals;
using BrokerOS.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Quotations;

public sealed class QuotationService : IQuotationService
{
    private readonly BrokerOsDbContext _dbContext;
    private readonly ICurrentUserService _currentUser;
    private readonly INotificationSender _notificationSender;
    private readonly IClock _clock;

    public QuotationService(
        BrokerOsDbContext dbContext,
        ICurrentUserService currentUser,
        INotificationSender notificationSender,
        IClock clock)
    {
        _dbContext = dbContext;
        _currentUser = currentUser;
        _notificationSender = notificationSender;
        _clock = clock;
    }

    public async Task<IReadOnlyList<QuotationDto>> ListForRenewalAsync(
        Guid renewalPublicId,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(renewalPublicId, asNoTracking: true, cancellationToken);
        var quotations = await AccessibleQuotations()
            .AsNoTracking()
            .Where(quotation => quotation.RenewalId == renewal.Id)
            .ToListAsync(cancellationToken);

        return MapMany(quotations, _clock.Today);
    }

    public async Task<QuotationDto> CreateAsync(
        Guid renewalPublicId,
        CreateQuotationRequest request,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(renewalPublicId, asNoTracking: false, cancellationToken);
        EnsureOpen(renewal);

        var insurer = await ResolveInsurerAsync(request.InsurerPublicId, request.NewInsurerName, cancellationToken);
        var quotation = new Quotation
        {
            OrganizationId = renewal.OrganizationId,
            RenewalId = renewal.Id,
            InsurerId = insurer.Id,
            Insurer = insurer,
            Renewal = renewal,
            PremiumAmount = request.PremiumAmount,
            SumInsured = request.SumInsured,
            CoverageSummary = request.CoverageSummary?.Trim() ?? string.Empty,
            ValidUntil = request.ValidUntil,
            Status = QuotationStatus.Received,
            Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim()
        };
        _dbContext.Quotations.Add(quotation);
        AddActivity(
            renewal,
            ActivityType.QuotationLogged,
            $"Quotation logged from {insurer.Name}: {QuotationShareFormatter.FormatInr(request.PremiumAmount)}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        return (await ListForRenewalAsync(renewalPublicId, cancellationToken))
            .Single(item => item.PublicId == quotation.PublicId);
    }

    public async Task<QuotationDto> UpdateAsync(
        Guid publicId,
        UpdateQuotationRequest request,
        CancellationToken cancellationToken)
    {
        var quotation = await GetAccessibleQuotationAsync(publicId, asNoTracking: false, cancellationToken);
        EnsureOpen(quotation.Renewal);

        var insurer = await ResolveInsurerAsync(request.InsurerPublicId, request.NewInsurerName, cancellationToken);
        quotation.InsurerId = insurer.Id;
        quotation.Insurer = insurer;
        quotation.PremiumAmount = request.PremiumAmount;
        quotation.SumInsured = request.SumInsured;
        quotation.CoverageSummary = request.CoverageSummary?.Trim() ?? string.Empty;
        quotation.ValidUntil = request.ValidUntil;
        quotation.Notes = string.IsNullOrWhiteSpace(request.Notes) ? null : request.Notes.Trim();

        await _dbContext.SaveChangesAsync(cancellationToken);
        return await MapOneAmongSiblings(quotation, cancellationToken);
    }

    public async Task<QuotationDto> SelectAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var quotation = await GetAccessibleQuotationAsync(publicId, asNoTracking: false, cancellationToken);
        EnsureOpen(quotation.Renewal);

        // One chosen option per renewal: the client (via the broker) picks a single quote to bind.
        // Selecting this row marks it Selected and every sibling on the same file Rejected.
        var siblings = await _dbContext.Quotations
            .Where(item => item.RenewalId == quotation.RenewalId)
            .ToListAsync(cancellationToken);

        foreach (var sibling in siblings)
        {
            sibling.Status = sibling.Id == quotation.Id
                ? QuotationStatus.Selected
                : QuotationStatus.Rejected;
        }

        AddActivity(
            quotation.Renewal,
            ActivityType.QuotationSelected,
            $"Selected {quotation.Insurer.Name} at {QuotationShareFormatter.FormatInr(quotation.PremiumAmount)}.");

        await _dbContext.SaveChangesAsync(cancellationToken);
        quotation.Status = QuotationStatus.Selected;
        return await MapOneAmongSiblings(quotation, cancellationToken);
    }

    public async Task DeleteAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var quotation = await GetAccessibleQuotationAsync(publicId, asNoTracking: false, cancellationToken);
        EnsureOpen(quotation.Renewal);
        _dbContext.Quotations.Remove(quotation);
        await _dbContext.SaveChangesAsync(cancellationToken);
    }

    public async Task<NotificationDto> ShareAsync(Guid publicId, CancellationToken cancellationToken)
    {
        var quotation = await GetAccessibleQuotationAsync(publicId, asNoTracking: false, cancellationToken);
        var renewal = quotation.Renewal;
        var (subject, body) = QuotationShareFormatter.ForOne(
            ClientName(renewal),
            OrganizationName(renewal),
            renewal.Policy.PolicyNumber,
            renewal.Policy.ExpiryDate,
            ToShareLine(quotation, isSelected: quotation.Status == QuotationStatus.Selected),
            AssignedName(renewal));

        return await SendShareAsync(renewal, subject, body, cancellationToken);
    }

    public async Task<NotificationDto> ShareComparisonAsync(
        Guid renewalPublicId,
        CancellationToken cancellationToken)
    {
        var renewal = await GetAccessibleRenewalAsync(renewalPublicId, asNoTracking: false, cancellationToken);
        var quotations = await AccessibleQuotations()
            .Where(quotation => quotation.RenewalId == renewal.Id)
            .ToListAsync(cancellationToken);

        if (quotations.Count == 0)
        {
            throw new BusinessRuleException("There are no quotations on this renewal to share.");
        }

        var (subject, body) = QuotationShareFormatter.ForComparison(
            ClientName(renewal),
            OrganizationName(renewal),
            renewal.Policy.PolicyNumber,
            renewal.Policy.ExpiryDate,
            quotations.Select(item => ToShareLine(item, item.Status == QuotationStatus.Selected)).ToList(),
            AssignedName(renewal));

        return await SendShareAsync(renewal, subject, body, cancellationToken);
    }

    private async Task<NotificationDto> SendShareAsync(
        Renewal renewal,
        string subject,
        string body,
        CancellationToken cancellationToken)
    {
        var notification = new Notification
        {
            OrganizationId = renewal.OrganizationId,
            RenewalId = renewal.Id,
            ClientId = renewal.Policy.ClientId,
            RecipientType = NotificationRecipientType.Client,
            Channel = NotificationChannel.WhatsApp,
            Subject = subject.Length > 200 ? subject[..200] : subject,
            Body = body,
            Status = NotificationStatus.Simulated,
            Organization = renewal.Organization,
            Renewal = renewal,
            Client = renewal.Policy.Client
        };

        await _notificationSender.SendAsync(notification, cancellationToken);
        AddActivity(
            renewal,
            ActivityType.QuotationShared,
            subject);
        await _dbContext.SaveChangesAsync(cancellationToken);
        return MapNotification(notification);
    }

    private async Task<Insurer> ResolveInsurerAsync(
        Guid? insurerPublicId,
        string? newInsurerName,
        CancellationToken cancellationToken)
    {
        if (insurerPublicId is Guid publicId && publicId != Guid.Empty)
        {
            var insurer = await _dbContext.Insurers
                .SingleOrDefaultAsync(item => item.PublicId == publicId, cancellationToken);
            AssignmentScope.EnsureFound(insurer);
            return insurer!;
        }

        var name = newInsurerName!.Trim();
        var existing = await _dbContext.Insurers
            .FirstOrDefaultAsync(
                item => item.OrganizationId == _currentUser.OrganizationId
                    && item.Name.ToLower() == name.ToLower(),
                cancellationToken);
        if (existing is not null)
        {
            return existing;
        }

        var created = new Insurer
        {
            OrganizationId = _currentUser.OrganizationId,
            Name = name,
            Code = GenerateInsurerCode(name),
            IsActive = true
        };
        _dbContext.Insurers.Add(created);
        return created;
    }

    private static string GenerateInsurerCode(string name)
    {
        var slug = new string(name.Where(char.IsLetterOrDigit).Take(8).ToArray()).ToUpperInvariant();
        if (slug.Length < 2)
        {
            slug = "INS";
        }

        return $"{slug}-{Guid.NewGuid().ToString("N")[..6].ToUpperInvariant()}";
    }

    private IQueryable<Quotation> AccessibleQuotations()
    {
        return _dbContext.Quotations
            .Include(quotation => quotation.Insurer)
            .Include(quotation => quotation.Renewal)
            .ForCurrentUser(_currentUser);
    }

    private async Task<Renewal> GetAccessibleRenewalAsync(
        Guid publicId,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Renewals
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Client)
            .Include(renewal => renewal.Policy)
                .ThenInclude(policy => policy.Insurer)
            .Include(renewal => renewal.Organization)
            .Include(renewal => renewal.AssignedUser)
            .ForCurrentUser(_currentUser)
            .Where(renewal => renewal.PublicId == publicId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var renewal = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(renewal);
        return renewal!;
    }

    private async Task<Quotation> GetAccessibleQuotationAsync(
        Guid publicId,
        bool asNoTracking,
        CancellationToken cancellationToken)
    {
        var query = _dbContext.Quotations
            .Include(quotation => quotation.Insurer)
            .Include(quotation => quotation.Renewal)
                .ThenInclude(renewal => renewal.Policy)
                    .ThenInclude(policy => policy.Client)
            .Include(quotation => quotation.Renewal)
                .ThenInclude(renewal => renewal.Policy)
                    .ThenInclude(policy => policy.Insurer)
            .Include(quotation => quotation.Renewal)
                .ThenInclude(renewal => renewal.Organization)
            .Include(quotation => quotation.Renewal)
                .ThenInclude(renewal => renewal.AssignedUser)
            .ForCurrentUser(_currentUser)
            .Where(quotation => quotation.PublicId == publicId);

        if (asNoTracking)
        {
            query = query.AsNoTracking();
        }

        var quotation = await query.SingleOrDefaultAsync(cancellationToken);
        AssignmentScope.EnsureFound(quotation);
        return quotation!;
    }

    private async Task<QuotationDto> MapOneAmongSiblings(Quotation quotation, CancellationToken cancellationToken)
    {
        var siblings = await AccessibleQuotations()
            .AsNoTracking()
            .Where(item => item.RenewalId == quotation.RenewalId)
            .ToListAsync(cancellationToken);

        return MapMany(siblings, _clock.Today).Single(item => item.PublicId == quotation.PublicId);
    }

    private static IReadOnlyList<QuotationDto> MapMany(IReadOnlyList<Quotation> quotations, DateOnly today)
    {
        var comparablePremiums = quotations
            .Where(item => EffectiveStatus(item, today) is QuotationStatus.Received
                or QuotationStatus.Selected
                or QuotationStatus.Expired)
            .Select(item => item.PremiumAmount)
            .ToList();
        var lowest = comparablePremiums.Count == 0 ? (decimal?)null : comparablePremiums.Min();

        return quotations
            .OrderByDescending(item => item.Status == QuotationStatus.Selected)
            .ThenBy(item => item.PremiumAmount)
            .Select(item =>
            {
                var status = EffectiveStatus(item, today);
                var isLowest = lowest.HasValue
                    && item.PremiumAmount == lowest.Value
                    && status is QuotationStatus.Received or QuotationStatus.Selected or QuotationStatus.Expired;
                return Map(item, status, isLowest);
            })
            .ToList();
    }

    private static QuotationDto Map(Quotation quotation, QuotationStatus status, bool isLowestPremium)
    {
        return new QuotationDto
        {
            PublicId = quotation.PublicId,
            RenewalPublicId = quotation.Renewal.PublicId,
            InsurerPublicId = quotation.Insurer.PublicId,
            InsurerName = quotation.Insurer.Name,
            PremiumAmount = quotation.PremiumAmount,
            SumInsured = quotation.SumInsured,
            CoverageSummary = quotation.CoverageSummary,
            ValidUntil = quotation.ValidUntil,
            Status = status.ToString(),
            Notes = quotation.Notes,
            IsLowestPremium = isLowestPremium,
            CreatedAtUtc = quotation.CreatedAtUtc,
            ModifiedAtUtc = quotation.ModifiedAtUtc
        };
    }

    private static QuotationStatus EffectiveStatus(Quotation quotation, DateOnly today)
    {
        if (quotation.Status == QuotationStatus.Received
            && quotation.ValidUntil is DateOnly until
            && until < today)
        {
            return QuotationStatus.Expired;
        }

        return quotation.Status;
    }

    private static QuotationShareFormatter.ShareLine ToShareLine(Quotation quotation, bool isSelected) =>
        new(
            quotation.Insurer.Name,
            quotation.PremiumAmount,
            quotation.SumInsured,
            quotation.CoverageSummary,
            quotation.ValidUntil,
            isSelected);

    private static NotificationDto MapNotification(Notification notification)
    {
        var renewal = notification.Renewal;
        var policy = renewal.Policy;
        var client = notification.Client ?? policy.Client;

        return new NotificationDto
        {
            PublicId = notification.PublicId,
            RenewalPublicId = renewal.PublicId,
            ClientPublicId = client?.PublicId,
            ClientName = client?.CompanyName,
            PolicyNumber = policy.PolicyNumber,
            OrganizationName = renewal.Organization.Name,
            RecipientType = notification.RecipientType.ToString(),
            Channel = notification.Channel.ToString(),
            RecipientName = client?.CompanyName ?? "Client",
            RecipientAddress = client?.Phone,
            Subject = notification.Subject,
            Body = notification.Body,
            Status = notification.Status.ToString(),
            ReminderMilestoneDays = notification.ReminderMilestoneDays,
            CreatedAtUtc = notification.CreatedAtUtc
        };
    }

    private void AddActivity(Renewal renewal, ActivityType activityType, string description)
    {
        _dbContext.Activities.Add(new Activity
        {
            OrganizationId = renewal.OrganizationId,
            ClientId = renewal.Policy.ClientId,
            PolicyId = renewal.PolicyId,
            RenewalId = renewal.Id,
            UserId = _currentUser.UserId,
            ActivityType = activityType,
            Description = description
        });
    }

    private static void EnsureOpen(Renewal renewal)
    {
        if (!RenewalFactory.IsOpen(renewal.Status))
        {
            throw new BusinessRuleException("This renewal is already closed.");
        }
    }

    private static string ClientName(Renewal renewal) =>
        string.IsNullOrWhiteSpace(renewal.Policy.Client?.CompanyName)
            ? "Client"
            : renewal.Policy.Client.CompanyName;

    private static string OrganizationName(Renewal renewal) =>
        string.IsNullOrWhiteSpace(renewal.Organization?.Name)
            ? "your broker"
            : renewal.Organization.Name;

    private static string? AssignedName(Renewal renewal) =>
        renewal.AssignedUser?.FullName;
}
