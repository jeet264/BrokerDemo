using BrokerOS.Application.Abstractions;
using BrokerOS.Domain.Common;
using BrokerOS.Domain.Entities;
using BrokerOS.Domain.Renewals;
using Microsoft.EntityFrameworkCore;

namespace BrokerOS.Infrastructure.Persistence;

public sealed class BrokerOsDbContext : DbContext
{
    private readonly ITenantContext _tenantContext;
    private readonly IClock _clock;

    public BrokerOsDbContext(
        DbContextOptions<BrokerOsDbContext> options,
        ITenantContext tenantContext,
        IClock clock)
        : base(options)
    {
        _tenantContext = tenantContext;
        _clock = clock;
    }

    public long CurrentOrganizationId => _tenantContext.OrganizationId ?? 0;

    public DbSet<Organization> Organizations => Set<Organization>();

    public DbSet<User> Users => Set<User>();

    public DbSet<Client> Clients => Set<Client>();

    public DbSet<Contact> Contacts => Set<Contact>();

    public DbSet<Insurer> Insurers => Set<Insurer>();

    public DbSet<Policy> Policies => Set<Policy>();

    public DbSet<Renewal> Renewals => Set<Renewal>();

    public DbSet<WorkTask> Tasks => Set<WorkTask>();

    public DbSet<Activity> Activities => Set<Activity>();

    public DbSet<Notification> Notifications => Set<Notification>();

    public DbSet<Quotation> Quotations => Set<Quotation>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(BrokerOsDbContext).Assembly);
        ApplyTenantAndSoftDeleteFilters(modelBuilder);
    }

    public override int SaveChanges()
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges();
    }

    public override int SaveChanges(bool acceptAllChangesOnSuccess)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChanges(acceptAllChangesOnSuccess);
    }

    public override Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(cancellationToken);
    }

    public override Task<int> SaveChangesAsync(bool acceptAllChangesOnSuccess, CancellationToken cancellationToken = default)
    {
        ApplyAuditAndSoftDelete();
        return base.SaveChangesAsync(acceptAllChangesOnSuccess, cancellationToken);
    }

    private void ApplyTenantAndSoftDeleteFilters(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Organization>().HasQueryFilter(entity => entity.Id == CurrentOrganizationId);

        modelBuilder.Entity<User>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Client>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Contact>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Insurer>().HasQueryFilter(entity =>
            CurrentOrganizationId != 0
            && (entity.OrganizationId == null || entity.OrganizationId == CurrentOrganizationId));

        modelBuilder.Entity<Policy>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Renewal>().HasQueryFilter(entity =>
            entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<WorkTask>().HasQueryFilter(entity =>
            !entity.IsDeleted && entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Activity>().HasQueryFilter(entity =>
            entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Notification>().HasQueryFilter(entity =>
            entity.OrganizationId == CurrentOrganizationId);

        modelBuilder.Entity<Quotation>().HasQueryFilter(entity =>
            entity.OrganizationId == CurrentOrganizationId);
    }

    private void ApplyAuditAndSoftDelete()
    {
        EnsureRenewalsForNewPolicies();

        var utcNow = _clock.UtcNow;
        var userIdentifier = string.IsNullOrWhiteSpace(_tenantContext.CurrentUserIdentifier)
            ? "system"
            : _tenantContext.CurrentUserIdentifier;

        foreach (var entry in ChangeTracker.Entries())
        {
            if (entry.State == EntityState.Deleted && entry.Entity is ISoftDeletable softDeletable)
            {
                entry.State = EntityState.Modified;
                softDeletable.IsDeleted = true;
            }

            if (entry.Entity is Entity entity && entity.PublicId == Guid.Empty)
            {
                entity.PublicId = Guid.NewGuid();
            }

            switch (entry.State)
            {
                case EntityState.Added:
                    if (entry.Entity is IAudited addedAudited)
                    {
                        if (addedAudited.CreatedAtUtc == default)
                        {
                            addedAudited.CreatedAtUtc = utcNow;
                        }

                        addedAudited.CreatedBy ??= userIdentifier;
                    }
                    else if (entry.Entity is Organization organization && organization.CreatedAtUtc == default)
                    {
                        organization.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Contact contact && contact.CreatedAtUtc == default)
                    {
                        contact.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Insurer insurer && insurer.CreatedAtUtc == default)
                    {
                        insurer.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Activity activity && activity.CreatedAtUtc == default)
                    {
                        activity.CreatedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Notification notification && notification.CreatedAtUtc == default)
                    {
                        notification.CreatedAtUtc = utcNow;
                    }

                    break;

                case EntityState.Modified:
                    if (entry.Entity is IAudited modifiedAudited)
                    {
                        modifiedAudited.ModifiedAtUtc = utcNow;
                        modifiedAudited.ModifiedBy = userIdentifier;
                    }
                    else if (entry.Entity is Organization modifiedOrganization)
                    {
                        modifiedOrganization.ModifiedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Contact modifiedContact)
                    {
                        modifiedContact.ModifiedAtUtc = utcNow;
                    }
                    else if (entry.Entity is Insurer modifiedInsurer)
                    {
                        modifiedInsurer.ModifiedAtUtc = utcNow;
                    }

                    break;
            }
        }
    }

    private void EnsureRenewalsForNewPolicies()
    {
        var addedPolicies = ChangeTracker.Entries<Policy>()
            .Where(entry => entry.State == EntityState.Added)
            .Select(entry => entry.Entity);

        var today = _clock.Today;
        foreach (var policy in addedPolicies)
        {
            if (policy.Renewals.Count > 0)
            {
                continue;
            }

            policy.Renewals.Add(RenewalFactory.CreateForPolicy(policy, today));
        }
    }
}
